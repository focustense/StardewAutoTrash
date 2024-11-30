using AutoTrash2.Config;
using AutoTrash2.Data;
using StardewValley;

namespace AutoTrash2.UI;

/// <summary>
/// View model for the trashable items menu.
/// </summary>
/// <param name="config">The current mod configuration.</param>
/// <param name="data">Mod data for the current game.</param>
/// <param name="location">The location to be configured, for local rules.</param>
public class TrashablesViewModel(Configuration config, TrashData data, GameLocation location)
{
    /// <summary>
    /// Text to display when there are no trashable item rules.
    /// </summary>
    public string EmptyText { get; } =
        I18n.TrashMenu_EmptyText(I18n.Options_ModifierKey_Label(), config.ModifierKey);

    /// <summary>
    /// Whether the list of rules is empty, i.e. whether to display the <see cref="EmptyText"/> instead of the grid.
    /// </summary>
    public bool IsEmpty { get; } = data.IsEmpty();

    /// <summary>
    /// List of trashable items, and their rules, to display in the grid.
    /// </summary>
    public IReadOnlyList<TrashItemViewModel> Items { get; } = CreateItemList(data, location);

    /// <summary>
    /// Menu title, to be displayed as a banner above the grid or empty text.
    /// </summary>
    public string Title { get; } = I18n.TrashMenu_Title(location.GetPathText());

    private static IReadOnlyList<TrashItemViewModel> CreateItemList(
        TrashData data,
        GameLocation location
    )
    {
        string locationKey = location.GetSemiUniqueKey();
        long playerId = Game1.player.UniqueMultiplayerID;
        return data.GetAllItemIds()
            .Select(ItemRegistry.GetData)
            .Where(itemData => itemData is not null)
            .Select(itemData => new TrashItemViewModel(data, itemData, locationKey, playerId))
            .ToList();
    }
}
