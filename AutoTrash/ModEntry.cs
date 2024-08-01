﻿using AutoTrash2.Config;
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

namespace AutoTrash2;

internal sealed class ModEntry : Mod
{
    private static readonly TimeSpan MIN_TRASH_SOUND_INTERVAL = TimeSpan.FromSeconds(1);

    // Initialized in Entry
    private Configuration config = null!;
    private Harmony harmony = null!;

    private TrashData currentData = new();
    private TimeSpan lastTrashSoundTime = TimeSpan.Zero;

    public override void Entry(IModHelper helper)
    {
        Logger.Monitor = Monitor;

        I18n.Init(helper.Translation);
        config = helper.ReadConfig<Configuration>();
        Sprites.ModMenuTexture = helper.ModContent.Load<Texture2D>("assets/menu.png");
        harmony = new(ModManifest.UniqueID);

        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.GameLoop.Saving += GameLoop_Saving;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        helper.Events.Player.InventoryChanged += Player_InventoryChanged;

        InventoryInterceptor.ConfigSelector = () => config;
        InventoryInterceptor.DataSelector = () => currentData;

        harmony.Patch(
            original: AccessTools.Method(typeof(Utility), nameof(Utility.trashItem)),
            postfix: new(typeof(TrashDetector), nameof(TrashDetector.Utility_trashItem_Postfix)));
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetItemReceiveBehavior)),
            postfix: new(
                typeof(InventoryInterceptor),
                nameof(InventoryInterceptor.Farmer_GetItemReceiveBehavior_Postfix)));
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
        TrackNewTrashables();
        TrashPendingItems();
    }

    private void Input_ButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.player.CanMove
            && !Game1.freezeControls
            && Game1.activeClickableMenu is null
            && config.MenuKey.JustPressed())
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
        if (config.MinEmptySlots <= 0 || !e.Added.Any())
        {
            return;
        }
        var inventory = Game1.player.Items;
        var maxSlots = Game1.player.MaxItems;
        var occupiedSlots = inventory.CountItemStacks();
        var availableSlots = maxSlots - occupiedSlots;
        var locationName = Game1.currentLocation.NameOrUniqueName;
        bool wasAnyTrashed = false;
        for (int i = inventory.Count - 1; i >= 0 && availableSlots < config.MinEmptySlots; i--)
        {
            var currentItem = inventory[i];
            if (currentItem is null)
            {
                continue;
            }
            if (currentData.IsTrash(locationName, currentItem.QualifiedItemId))
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
        var locationName = Game1.currentLocation.NameOrUniqueName;
        foreach (var item in TrashDetector.DetectedItems)
        {
            currentData.SetTrashFlag(locationName, item.QualifiedItemId, true);
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
