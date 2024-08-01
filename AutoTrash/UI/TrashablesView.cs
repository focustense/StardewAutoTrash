using AutoTrash2.Config;
using AutoTrash2.Data;
using Microsoft.Xna.Framework;
using StardewUI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace AutoTrash2.UI;

internal class TrashablesView(Configuration config, TrashData data, GameLocation location) : WrapperView
{
    public Configuration Config { get; } = config;
    public TrashData Data { get; } = data;
    public GameLocation Location { get; } = location;

    public List<ParsedItemData> AllItems { get; } = data.GetAllItemIds()
        .Select(id => ItemRegistry.GetDataOrErrorItem(id))
        .OrderBy(x => x.Category)
        .ThenBy(x => x.DisplayName)
        .ToList();

    protected override IView CreateView()
    {
        var content = CreateItemGridOrEmptyText();
        var legend = CreateLegend();
        return new ScrollableFrameView()
        {
            Name = "TrashablesRoot",
            FrameLayout = LayoutParameters.FixedSize(1280, 800),
            Title = I18n.TrashMenu_Title(Location.DisplayName),
            Content = content,
            Footer = legend,
        };
    }

    private IView CreateGridItem(ParsedItemData itemData)
    {
        var panel = new Panel()
        {
            Name = $"{itemData.InternalName}_GridItem",
            Layout = LayoutParameters.FitContent(),
            HorizontalContentAlignment = Alignment.End,
            VerticalContentAlignment = Alignment.End,
            Tooltip = itemData.DisplayName,
            Tags = Tags.Create(itemData),
        };
        UpdateGridItem(panel);
        panel.Click += GridItem_Click;
        return panel;
    }

    private IView CreateItemGridOrEmptyText()
    {
        if (AllItems.Count == 0)
        {
            return new Label()
            {
                Layout = LayoutParameters.AutoRow(),
                Padding = new(8),
                Text = I18n.TrashMenu_EmptyText(I18n.Options_ModifierKey_Label(), Config.ModifierKey),
            };
        }
        return new Grid()
        {
            Name = "ItemGrid",
            Layout = LayoutParameters.AutoRow(),
            Padding = new(8),
            ItemLayout = GridItemLayout.Length(64),
            ItemSpacing = new(16, 16),
            Children = AllItems.Select(CreateGridItem).ToList(),
        };
    }

    private IView CreateLegend()
    {
        return new Frame()
        {
            Name = "Legend",
            Layout = LayoutParameters.FitContent(),
            Padding = (Sprites.ControlBorder.FixedEdges ?? Edges.NONE) + new Edges(8),
            Background = Sprites.ControlBorder,
            Content = new Lane()
            {
                Layout = LayoutParameters.FitContent(),
                VerticalContentAlignment = Alignment.Middle,
                Children = [
                    Label.Simple(I18n.TrashMenu_Legend_Title()),
                    new Image()
                    {
                        Layout = LayoutParameters.FixedSize(27, 27),
                        Margin = new(Left: 24, Right: 8),
                        Sprite = Sprites.TrashOn,
                    },
                    Label.Simple(I18n.TrashMenu_Legend_Local()),
                    new Image()
                    {
                        Layout = LayoutParameters.FixedSize(27, 27),
                        Margin = new(Left: 16, Right: 8),
                        Sprite = Sprites.TrashGlobal,
                    },
                    Label.Simple(I18n.TrashMenu_Legend_Global()),
                ],
            }
        };
    }

    private void GridItem_Click(object? sender, ClickEventArgs e)
    {
        if (sender is not Panel panel || panel.Tags.Get<ParsedItemData>() is not ParsedItemData itemData)
        {
            return;
        }
        Game1.playSound("smallSelect");
        if (e.IsSecondaryButton())
        {
            var isGlobalTrash = Data.GlobalFilter.ItemIds.Contains(itemData.QualifiedItemId);
            Data.SetGlobalTrashFlag(itemData.QualifiedItemId, !isGlobalTrash);
        }
        else
        {
            var isLocalTrash = Data.FiltersByLocationName.TryGetValue(Location.NameOrUniqueName, out var filter)
                && filter.ItemIds.Contains(itemData.QualifiedItemId);
            Data.SetTrashFlag(Location.NameOrUniqueName, itemData.QualifiedItemId, !isLocalTrash);
        }
        UpdateGridItem(panel);
    }

    private void UpdateGridItem(Panel panel)
    {
        var itemData = panel.Tags.Get<ParsedItemData>()!;
        var isLocalTrash = Data.FiltersByLocationName.TryGetValue(Location.NameOrUniqueName, out var filter)
            && filter.ItemIds.Contains(itemData.QualifiedItemId);
        var isGlobalTrash = Data.GlobalFilter.ItemIds.Contains(itemData.QualifiedItemId);
        var isLocalOrGlobalTrash = isLocalTrash || isGlobalTrash;
        var image = new Image()
        {
            Name = itemData.InternalName,
            Layout = LayoutParameters.FixedSize(64, 64),
            Sprite = Sprites.Item(itemData),
            Tint = isLocalOrGlobalTrash ? Color.White : new(128, 128, 128, 128),
            ShadowAlpha = isLocalOrGlobalTrash ? 0.5f : 0.05f,
            ShadowOffset = new(-4, 4),
            HorizontalAlignment = Alignment.Middle,
            VerticalAlignment = Alignment.Middle,
            IsFocusable = true,
        };
        var localCheckbox = new Image()
        {
            Name = $"{itemData.InternalName}_LocalCheckbox",
            Layout = LayoutParameters.FixedSize(18, 18),
            Sprite = isLocalTrash ? Sprites.TrashOn : Sprites.TrashOff,
            Tint = isLocalTrash ? Color.White : new(48, 48, 48, 48),
            ZIndex = 1,
        };
        var globalCheckbox = new Image()
        {
            Name = $"{itemData.InternalName}_GlobalCheckbox",
            Margin = new(Right: 24),
            Layout = LayoutParameters.FixedSize(18, 18),
            Sprite = Sprites.TrashGlobal,
            Visibility = isGlobalTrash ? Visibility.Visible : Visibility.Hidden,
            ZIndex = 1,
        };
        panel.Children = [image, localCheckbox, globalCheckbox];
    }
}
