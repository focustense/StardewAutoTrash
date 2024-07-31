using AutoTrash2.Config;
using AutoTrash2.Data;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<Configuration>();
        harmony = new(ModManifest.UniqueID);

        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.GameLoop.Saving += GameLoop_Saving;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

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
            Monitor.Log("Menu button pressed", LogLevel.Info);
            Helper.Input.SuppressActiveKeybinds(config.MenuKey);
        }
        TrashDetector.Detecting = config.ModifierKey.IsDown();
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
        var gameTime = Game1.currentGameTime.TotalGameTime;
        if (gameTime - lastTrashSoundTime >= MIN_TRASH_SOUND_INTERVAL)
        {
            Game1.playSound("trashcan");
            lastTrashSoundTime = gameTime;
        }
    }
}
