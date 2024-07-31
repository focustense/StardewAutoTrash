using AutoTrash2.Config;
using AutoTrash2.Data;
using StardewValley;

namespace AutoTrash2;

/// <summary>
/// Intercepts items being added to inventory and moves them to a temporary trash buffer to be handled by the mod.
/// </summary>
/// <remarks>
/// It is possible to auto-trash items after they've been added to the player's inventory using SMAPI events, i.e.
/// <see cref="StardewModdingAPI.Events.IPlayerEvents.InventoryChanged"/>. However, this event runs sometime after the
/// changes are visible, making it impossible to suppress the original item-received notification or avoid flickering in
/// the toolbar. To actually prevent the item from ever entering inventory, we have to patch.
/// </remarks>
internal static class InventoryInterceptor
{
    /// <summary>
    /// Function to get the current mod configuration, usually set by <see cref="ModEntry"/> on initialization.
    /// </summary>
    public static Func<Configuration>? ConfigSelector { get; set; }

    /// <summary>
    /// Function to get the current trash data, usually set by <see cref="ModEntry"/> on load. Holds the filters that
    /// determine which items are considered trash.
    /// </summary>
    public static Func<TrashData>? DataSelector { get; set; }

    /// <summary>
    /// List of all trash items that have been intercepted and not yet handled.
    /// </summary>
    public static List<Item> InterceptedItems { get; set; } = [];

    public static void Farmer_GetItemReceiveBehavior_Postfix(
        Item item,
        ref bool needsInventorySpace,
        ref bool showNotification)
    {
        var config = ConfigSelector?.Invoke();
        var data = DataSelector?.Invoke();
        if (config is null || data is null)
        {
            return;
        }
        var locationName = Game1.currentLocation.NameOrUniqueName;
        if (data.IsTrash(locationName, item.QualifiedItemId))
        {
            // Farmer has many different "addItemToInventory" methods but they all have similar logic, in which they
            // check the result of needsInventorySpace and go directly to OnItemReceived if it's false, skipping all the
            // code that does the real adding.
            //
            // In this instance, that is a good thing, because we still want any other logic around item pickup (mail
            // flags, quest progress, player stats tracking, etc.) to run, which it will.
            needsInventorySpace = false;
            if (config.SuppressPickupNotification)
            {
                showNotification = false;
            }

            InterceptedItems.Add(item);
        }
    }
}
