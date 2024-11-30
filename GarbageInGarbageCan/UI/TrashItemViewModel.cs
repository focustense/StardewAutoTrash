using System.ComponentModel;
using AutoTrash2.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace AutoTrash2.UI;

/// <summary>
/// View model for an item displayed in the trashable items view.
/// </summary>
/// <param name="data">Mod data for the current player and savegame.</param>
/// <param name="item">The base item data.</param>
/// <param name="locationKey">Unique key for the player's current location, used for local rules.</param>
/// <param name="playerId">Unique identifier of the player opening the menu. Affects displayed sell values.</param>
public partial class TrashItemViewModel(
    TrashData data,
    ParsedItemData item,
    string locationKey,
    long playerId
) : INotifyPropertyChanged
{
    /// <summary>
    /// Whether the item is declared as trash for all locations.
    /// </summary>
    public bool IsGlobalTrash => data.GlobalFilter.ItemIds.Contains(item.QualifiedItemId);

    /// <summary>
    /// Whether the item is declared as trash for the current location.
    /// </summary>
    /// <remarks>
    /// This is not inclusive of <see cref="IsGlobalTrash"/>. The per-location rule is separate from the global rule,
    /// and continues to apply even if any active global rule is removed.
    /// </remarks>
    public bool IsLocalTrash =>
        data.FiltersByLocationName.TryGetValue(locationKey, out var filter)
        && filter.ItemIds.Contains(item.QualifiedItemId);

    /// <summary>
    /// Whether the item is part of any trash rules, global or per-location.
    /// </summary>
    public bool IsTrash => IsGlobalTrash || IsLocalTrash;

    /// <summary>
    /// Opacity of the shadow displayed for the item image.
    /// </summary>
    public float ShadowAlpha => IsTrash ? 0.5f : 0.05f;

    /// <summary>
    /// Sprite to display as the item image.
    /// </summary>
    public Tuple<Texture2D, Rectangle> Sprite { get; } =
        Tuple.Create(item.GetTexture(), item.GetSourceRect());

    /// <summary>
    /// Tint color to apply to the <see cref="Sprite"/> image.
    /// </summary>
    public Color Tint => IsTrash ? Color.White : Color.White * 0.5f;

    /// <summary>
    /// Tooltip data to display when hovering over the item.
    /// </summary>
    [Notify]
    private TrashItemTooltip tooltip = GetTooltip(item, playerId);

    private bool wasGamepadControls = Game1.options.gamepadControls;

    /// <summary>
    /// Toggles the trashable status of this item in the global rules (i.e. for all locations).
    /// </summary>
    public void ToggleGlobal()
    {
        Game1.playSound("smallSelect");
        data.SetGlobalTrashFlag(item.QualifiedItemId, !IsGlobalTrash);
        OnPropertyChanged(new(nameof(IsGlobalTrash)));
    }

    /// <summary>
    /// Toggles the trashable status of this item in the rules for the current location.
    /// </summary>
    public void ToggleLocal()
    {
        Game1.playSound("smallSelect");
        data.SetTrashFlag(locationKey, item.QualifiedItemId, !IsLocalTrash);
        OnPropertyChanged(new(nameof(IsLocalTrash)));
    }

    /// <summary>
    /// Runs on every update tick. Handles transitions between input devices.
    /// </summary>
    public void Update()
    {
        if (Game1.options.gamepadControls == wasGamepadControls)
        {
            return;
        }
        Tooltip = GetTooltip(item, playerId);
        wasGamepadControls = Game1.options.gamepadControls;
    }

    private static TrashItemTooltip GetTooltip(ParsedItemData item, long playerId)
    {
        return TrashItemTooltip.ForItem(
            item.QualifiedItemId,
            playerId,
            Game1.options.gamepadControls
        );
    }
}
