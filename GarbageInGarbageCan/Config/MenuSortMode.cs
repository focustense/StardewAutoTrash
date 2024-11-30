namespace AutoTrash2.Config;

/// <summary>
/// Sort modes applicable to the trash rules menu.
/// </summary>
public enum MenuSortMode
{
    /// <summary>
    /// Default (alphabetical) sorting.
    /// </summary>
    Default,

    /// <summary>
    /// Show active rules (globally or in current location) first, then inactive rules, with each group using its own
    /// default sorting.
    /// </summary>
    ActiveFirst,

    /// <summary>
    /// Show rules active in the current location, then globally active, then inactive, each group using the default
    /// sorting.
    /// </summary>
    ActiveLocalFirst,
}
