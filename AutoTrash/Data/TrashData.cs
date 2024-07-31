namespace AutoTrash2.Data;

/// <summary>
/// Root for savegame data.
/// </summary>
internal class TrashData
{
    /// <summary>
    /// Location-specific filters, only applicable when the player is in that location.
    /// </summary>
    public Dictionary<string, TrashFilter> FiltersByLocationName = [];

    /// <summary>
    /// Filter applicable at all times, regardless of current location.
    /// </summary>
    public TrashFilter GlobalFilter { get; set; } = new();

    /// <summary>
    /// Checks whether or not a given item is considered trash.
    /// </summary>
    /// <param name="locationName">Unique name of the current location.</param>
    /// <param name="itemId">Qualified ID of the item to check.</param>
    /// <returns><c>true</c> if items with the given <paramref name="itemId"/> are flagged as trash, either globally or
    /// in the specified <paramref name="locationName"/>, otherwise <c>false</c>.</returns>
    public bool IsTrash(string locationName, string itemId)
    {
        return GlobalFilter.ItemIds.Contains(itemId)
            || (FiltersByLocationName.TryGetValue(locationName, out var filter) && filter.ItemIds.Contains(itemId));
    }

    /// <summary>
    /// Marks an item as trash (or not trash) for a given location.
    /// </summary>
    /// <param name="locationName">The unique location name.</param>
    /// <param name="itemId">Qualified ID of the item to flag.</param>
    /// <param name="isTrash">Whether or not item's with the specified <paramref name="itemId"/> should be considered
    /// trash.</param>
    public void SetTrashFlag(string locationName, string itemId, bool isTrash)
    {
        if (!FiltersByLocationName.TryGetValue(locationName, out var filter))
        {
            filter = new();
            FiltersByLocationName.Add(locationName, filter);
        }
        if (isTrash)
        {
            filter.ItemIds.Add(itemId);
        }
        else
        {
            filter.ItemIds.Remove(itemId);
        }
    }
}
