namespace AutoTrash2.Data;

/// <summary>
/// Root for savegame data.
/// </summary>
public class TrashData
{
    /// <summary>
    /// Location-specific filters, only applicable when the player is in that location.
    /// </summary>
    public Dictionary<string, TrashFilter> FiltersByLocationName { get; set; } = [];

    /// <summary>
    /// Filter applicable at all times, regardless of current location.
    /// </summary>
    public TrashFilter GlobalFilter { get; set; } = new();

    /// <summary>
    /// Returns a sequence of all item IDs tracked in any location/filter. Used for UI.
    /// </summary>
    /// <remarks>
    /// Must concatenate and dedupe, so don't call every frame.
    /// </remarks>
    public IEnumerable<string> GetAllItemIds()
    {
        return GlobalFilter
            .ItemIds.Concat(FiltersByLocationName.Values.SelectMany(filter => filter.ItemIds))
            .Distinct();
    }

    /// <summary>
    /// Checks if the data is empty, i.e. has no trashable items either globally or for any location.
    /// </summary>
    /// <returns><c>true</c> if there are no trashables defined anywhere, otherwise <c>false</c>.</returns>
    public bool IsEmpty()
    {
        return GlobalFilter.ItemIds.Count == 0
            || FiltersByLocationName.Values.All(filter => filter.ItemIds.Count == 0);
    }

    /// <summary>
    /// Checks whether a given item is considered trash.
    /// </summary>
    /// <param name="locationName">Unique name of the current location.</param>
    /// <param name="itemId">Qualified ID of the item to check.</param>
    /// <returns><c>true</c> if items with the given <paramref name="itemId"/> are flagged as trash, either globally or
    /// in the specified <paramref name="locationName"/>, otherwise <c>false</c>.</returns>
    public bool IsTrash(string locationName, string itemId)
    {
        return GlobalFilter.ItemIds.Contains(itemId)
            || (
                FiltersByLocationName.TryGetValue(locationName, out var filter)
                && filter.ItemIds.Contains(itemId)
            );
    }

    /// <summary>
    /// Marks an item as trash (or not trash) for all locations.
    /// </summary>
    /// <remarks>
    /// The setting is independent of location-specific settings configured by <see cref="SetTrashFlag"/>, so toggling
    /// this on and off won't erase previous location settings.
    /// </remarks>
    /// <param name="itemId">Qualified ID of the item to flag.</param>
    /// <param name="isTrash">Whether items with the specified <paramref name="itemId"/> should be considered
    /// trash.</param>
    public void SetGlobalTrashFlag(string itemId, bool isTrash)
    {
        if (isTrash)
        {
            GlobalFilter.ItemIds.Add(itemId);
        }
        else
        {
            GlobalFilter.ItemIds.Remove(itemId);
        }
    }

    /// <summary>
    /// Marks an item as trash (or not trash) for a given location.
    /// </summary>
    /// <param name="locationName">The unique location name.</param>
    /// <param name="itemId">Qualified ID of the item to flag.</param>
    /// <param name="isTrash">Whether items with the specified <paramref name="itemId"/> should be considered
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
