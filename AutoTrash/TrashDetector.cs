using StardewValley;

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
