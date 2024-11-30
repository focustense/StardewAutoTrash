namespace AutoTrash2.Data;

/// <summary>
/// Trash filter, i.e. criteria for whether an item should be automatically discarded.
/// </summary>
public class TrashFilter
{
    /// <summary>
    /// List of trash item IDs. Any items in this list are considered trash.
    /// </summary>
    public HashSet<string> ItemIds { get; set; } = [];
}
