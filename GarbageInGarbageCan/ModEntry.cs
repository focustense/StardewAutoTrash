using AutoTrash2.Config;
using AutoTrash2.Data;
using AutoTrash2.Integrations;
using AutoTrash2.Integrations.Gmcm;
using AutoTrash2.UI;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewUI;
using StardewValley;
using StardewValley.Menus;

namespace AutoTrash2;

internal sealed class ModEntry : Mod
{
    private static readonly TimeSpan MIN_TRASH_SOUND_INTERVAL = TimeSpan.FromSeconds(1);

    // Initialized in Entry
    private Configuration config = null!;
    private Harmony harmony = null!;
    private RecoveryBin recoveryBin = null!;

    private TrashData currentData = new();
    private TimeSpan lastTrashSoundTime = TimeSpan.Zero;

    public override void Entry(IModHelper helper)
    {
        Logger.Monitor = Monitor;

        I18n.Init(helper.Translation);
        config = helper.ReadConfig<Configuration>();
        Sprites.ModMenuTexture = helper.ModContent.Load<Texture2D>("assets/menu.png");

        recoveryBin = new(() => config.RecoveryLimit);

        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.GameLoop.Saving += GameLoop_Saving;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        helper.Events.Player.InventoryChanged += Player_InventoryChanged;

        InventoryInterceptor.ConfigSelector = () => config;
        InventoryInterceptor.DataSelector = () => currentData;

        harmony = new(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Utility), nameof(Utility.trashItem)),
            postfix: new(typeof(TrashDetector), nameof(TrashDetector.Utility_trashItem_Postfix)));
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetItemReceiveBehavior)),
            postfix: new(
                typeof(InventoryInterceptor),
                nameof(InventoryInterceptor.Farmer_GetItemReceiveBehavior_Postfix)));
        harmony.Patch(
            original: AccessTools.Method(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick)),
            prefix: new(typeof(TrashDetector), nameof(TrashDetector.InventoryPage_receiveLeftClick_Prefix)),
            postfix: new(typeof(TrashDetector), nameof(TrashDetector.InventoryPage_receiveLeftClick_Postfix)));
    }

    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Apis.LoadAll(Helper.ModRegistry);
        ConfigMenu.Register(
            ModManifest,
            config: () => config,
            reset: () => config = new(),
            save: () => Helper.WriteConfig(config));
    }

    // Event handlers

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        currentData = Helper.Data.ReadSaveData<TrashData>(ModManifest.UniqueID) ?? new();
    }

    private void GameLoop_Saving(object? sender, SavingEventArgs e)
    {
        Helper.Data.WriteSaveData(ModManifest.UniqueID, currentData);
    }

    private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }
        if (Context.IsPlayerFree)
        {
            recoveryBin.Trim(Game1.currentGameTime.ElapsedGameTime);
        }
        TrackNewTrashables();
        TrashPendingItems();
        if (TrashDetector.IsRecoveryRequested)
        {
            ShowRecoveryMenu();
            TrashDetector.IsRecoveryRequested = false;
        }
    }

    private void Input_ButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Context.IsPlayerFree && config.MenuKey.JustPressed())
        {
            ShowTrashMenu();
            Helper.Input.SuppressActiveKeybinds(config.MenuKey);
        }
        TrashDetector.Detecting = config.ModifierKey.IsDown();
    }

    private void Player_InventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        // We don't actually have to abort if items are only being removed and not added; however, it seems confusing to
        // start trashing items in the inventory just because the player dropped/trashed a different item. To keep the
        // behavior easy to understand, only start trashing when the inventory is filling up.
        //
        // Note that this method is only concerned with trash items that were already in the player's inventory due to a
        // permissive `MinEmptySlots`, and subsequently going over that limit with non-trash items. Harmony patches for
        // the Farmer already cover the scenario of "blocking" trash items from getting into the inventory when the
        // empty slot minimum is not configured or when a non-stackable trash item would go above the limit.
        if (!e.Added.Any())
        {
            return;
        }
        // In general, most inventory additions should be just one item and not a whole set, so checking for ANY item
        // bypassed could leave fewer empty slots than desired. However, it's important to do this to avoid "UI loops"
        // that seem strange and unintuitive, such as restoring one stack only to have a different stack immediately
        // ejected, then trying to restore the other stack and losing the first stack again.
        // Instead we boil it down to: "if this event is part of a recovery, ignore all trash rules until it ends".
        if (config.MinEmptySlots <= 0 || e.Added.Any(item => item.IsTrashCheckBypassed()))
        {
            RemoveOverrideFlags(e.Added);
            return;
        }
        var inventory = Game1.player.Items;
        var maxSlots = Game1.player.MaxItems;
        var occupiedSlots = inventory.CountItemStacks();
        var availableSlots = maxSlots - occupiedSlots;
        var locationKey = Game1.currentLocation.GetSemiUniqueKey();
        bool wasAnyTrashed = false;
        for (int i = inventory.Count - 1; i >= 0 && availableSlots < config.MinEmptySlots; i--)
        {
            var currentItem = inventory[i];
            if (currentItem is null)
            {
                continue;
            }
            if (currentData.IsTrash(locationKey, currentItem.QualifiedItemId))
            {
                inventory[i] = null;
                InternalTrashItem(currentItem);
                availableSlots++;
                wasAnyTrashed = true;
            }
        }
        if (wasAnyTrashed)
        {
            MaybePlayTrashSound();
        }
        RemoveOverrideFlags(e.Added);
    }

    // Core logic

    private void InternalTrashItem(Item item)
    {
        // There are a few reasons to reimplement this instead of using `Utility.trashItem`:
        // - It avoids a potential conflict with our own Harmony patch (and other mods)
        // - `Utility` doesn't actually remove the item from inventory, which we need to do
        // - We can better control the sound, and avoid playing the trash sound 40 times in a row
        // - Showing notifications if enabled
        var reclamationPrice = Utility.getTrashReclamationPrice(item, Game1.player);
        if (reclamationPrice > 0)
        {
            Game1.player.Money += reclamationPrice;
        }
        recoveryBin.Add(item);
        if (config.EnableTrashNotification)
        {
            // Unlike the notification in TrackTrashedItems, we actually want to use the specific name of the item here
            // since that is the actual item being discarded, not just the filter criteria.
            Game1.addHUDMessage(new(I18n.Hud_ItemTrashed(item.DisplayName))
            {
                type = $"AutoTrash_{item.Name}",
                messageSubject = item,
                number = item.Stack,
            });
        }
    }

    private void MaybePlayTrashSound()
    {
        var gameTime = Game1.currentGameTime.TotalGameTime;
        if (gameTime - lastTrashSoundTime >= MIN_TRASH_SOUND_INTERVAL)
        {
            Game1.playSound("trashcan");
            lastTrashSoundTime = gameTime;
        }
    }

    private void OnItemRecovered(Item item)
    {
        var reclamationPrice = Utility.getTrashReclamationPrice(item, Game1.player);
        if (reclamationPrice > 0)
        {
            Game1.player.Money -= reclamationPrice;
        }
    }

    private static void RemoveOverrideFlags(IEnumerable<Item> items)
    {
        // If an item was just recovered, remove its marker flag so that it's not skipped again.
        foreach (var item in items)
        {
            item.SetTrashCheckBypass(false);
        }
    }

    private void ShowRecoveryMenu()
    {
        if (recoveryBin.IsEmpty())
        {
            Game1.addHUDMessage(new(I18n.Hud_RecoveryUnavailable(), HUDMessage.error_type));
            return;
        }
        Game1.activeClickableMenu = new RecoveryMenu(recoveryBin.GetItems(), OnItemRecovered);
    }

    private void ShowTrashMenu()
    {
        Game1.activeClickableMenu = new TrashMenu(config, currentData, Game1.currentLocation);
    }

    private void TrackNewTrashables()
    {
        if (TrashDetector.DetectedItems.Count == 0)
        {
            return;
        }
        var locationKey = Game1.currentLocation.GetSemiUniqueKey();
        foreach (var item in TrashDetector.DetectedItems)
        {
            currentData.SetTrashFlag(locationKey, item.QualifiedItemId, true);
            // The item's display name could be more specific than the item ID, and we don't want to confuse the player.
            // Use the generic name for any item with that ID.
            var itemName = ItemRegistry.Create(item.QualifiedItemId).DisplayName;
            Game1.addHUDMessage(new(I18n.Hud_ItemFlagged(itemName))
            {
                type = $"TrashDetected_{item.Name}",
                messageSubject = item
            });
        }
        TrashDetector.DetectedItems.Clear();
    }

    private void TrashPendingItems()
    {
        if (InventoryInterceptor.InterceptedItems.Count == 0)
        {
            return;
        }
        foreach (var item in InventoryInterceptor.InterceptedItems)
        {
            InternalTrashItem(item);
        }
        InventoryInterceptor.InterceptedItems.Clear();
        MaybePlayTrashSound();
    }
}
