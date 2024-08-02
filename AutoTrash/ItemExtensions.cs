using StardewValley;

namespace AutoTrash2;

/// <summary>
/// Extension methods for the <see cref="Item"/> type.
/// </summary>
internal static class ItemExtensions
{
    /// <summary>
    /// Marker key that, when present in <see cref="Item.tempData"/>, will cause the trash check to be skipped.
    /// Used to temporarily prevent re-trashing items that are being recovered.
    /// </summary>
    internal const string TRASH_BYPASS_KEY = "focustense.AutoTrash2.SkipTrashCheck";

    /// <summary>
    /// Checks if an item should bypass trash checks, e.g. because it was just recovered.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns><c>true</c> if trash checks are bypassed for the <paramref name="item"/>, otherwise
    /// <c>false</c>.</returns>
    public static bool IsTrashCheckBypassed(this Item item)
    {
        return item.tempData?.ContainsKey(TRASH_BYPASS_KEY) == true;
    }

    /// <summary>
    /// Configures an item to start or stop bypassing trash checks.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="bypass"><c>true</c> to bypass trash checks; <c>false</c> to perform normal checks.</param>
    public static void SetTrashCheckBypass(this Item item, bool bypass)
    {
        if (bypass)
        {
            item.SetTempData(TRASH_BYPASS_KEY, true);
        }
        else
        {
            item.tempData?.Remove(TRASH_BYPASS_KEY);
        }
    }
}
