using StardewValley;
using StardewValley.Menus;

namespace AutoTrash2;

/// <summary>
/// Monitors newly-trashed items to track as auto-trashable.
/// </summary>
internal static class TrashDetector
{
    /// <summary>
    /// Trashed items detected this frame. The mod must read from and clear this in order to complete tracking.
    /// </summary>
    public static List<Item> DetectedItems { get; set; } = [];

    /// <summary>
    /// Whether or not trashed items should be detected and tracked this frame.
    /// </summary>
    /// <remarks>
    /// In practice, this is whether or not the auto-trash <see cref="Config.Configuration.ModifierKey"/> is down, but
    /// we don't share that level of detail.
    /// </remarks>
    public static bool Detecting { get; set; } = false;

    /// <summary>
    /// Whether or not we detected that the player wants to recover a trashed item.
    /// </summary>
    public static bool IsRecoveryRequested { get; set; } = false;

    /// <summary>
    /// Prefix patch for <see cref="InventoryPage.receiveLeftClick(int, int, bool)"/>.
    /// </summary>
    /// <remarks>
    /// Used to obtain the currently-held item before the postfix runs, since it may no longer be held on postfix.
    /// </remarks>
    public static void InventoryPage_receiveLeftClick_Prefix(ref Item __state)
    {
        __state = Game1.player.CursorSlotItem;
    }

    /// <summary>
    /// Postfix patch for <see cref="InventoryPage.receiveLeftClick(int, int, bool)"/>.
    /// </summary>
    public static void InventoryPage_receiveLeftClick_Postfix(
        int x,
        int y,
        ref Item __state,
        ref ClickableTextureComponent ___trashCan
    )
    {
        if (Detecting && ___trashCan.containsPoint(x, y) && __state is null)
        {
            IsRecoveryRequested = true;
        }
    }

    /// <summary>
    /// Postfix patch for <see cref="Utility.trashItem(Item)"/>.
    /// </summary>
    public static void Utility_trashItem_Postfix(Item item)
    {
        if (Detecting)
        {
            DetectedItems.Add(item);
        }
    }
}
