using Microsoft.Xna.Framework;
using StardewUI;
using StardewValley;

namespace AutoTrash2.UI;

internal class ScrollableFrameView : WrapperView
{
    public IView? Content
    {
        get => contentFrame?.Content;
        set
        {
            var _ = Root; // Ensure view created
            contentFrame.Content = value;
            scrollbar.Container = value is IScrollTarget target ? target.ScrollContainer : null;
        }
    }

    public IView? Footer
    {
        get => footerContainer?.Children.FirstOrDefault();
        set
        {
            var _ = Root; // Ensure view created
            footerContainer.Children = value is not null ? [value] : [];
        }
    }

    public LayoutParameters FrameLayout
    {
        get => contentFrame?.Layout ?? default;
        set
        {
            var _ = Root; // Ensure view created
            contentFrame.Layout = value;
        }
    }

    public string Title
    {
        get => banner?.Text ?? "";
        set
        {
            var _ = Root; // Ensure view created
            banner.Text = value;
        }
    }

    // Initialized in CreateView
    private Banner banner = null!;
    private Frame contentFrame = null!;
    private Panel footerContainer = null!;
    private Scrollbar scrollbar = null!;
    private Lane scrollingLayout = null!;

    public override bool Measure(Vector2 availableSize)
    {
        var wasDirty = base.Measure(availableSize);
        if (wasDirty)
        {
            footerContainer.Margin = new(Top: (int)MathF.Ceiling(scrollingLayout.OuterSize.Y));
        }
        return wasDirty;
    }

    public override void OnWheel(WheelEventArgs e)
    {
        if (e.Handled || scrollbar.Container is not ScrollContainer container)
        {
            return;
        }
        switch (e.Direction)
        {
            case Direction.North when container.Orientation == Orientation.Vertical:
            case Direction.West when container.Orientation == Orientation.Horizontal:
                e.Handled = container.ScrollBackward();
                break;
            case Direction.South when container.Orientation == Orientation.Vertical:
            case Direction.East when container.Orientation == Orientation.Horizontal:
                e.Handled = container.ScrollForward();
                break;
        }
        if (e.Handled)
        {
            Game1.playSound("shwip");
        }
    }

    protected override IView CreateView()
    {
        banner = new Banner()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(Top: -80),
            Padding = new(12),
            Background = Sprites.BannerBackground,
            BackgroundBorderThickness = (Sprites.BannerBackground.FixedEdges ?? Edges.NONE)
                * (Sprites.BannerBackground.SliceSettings?.Scale ?? 1),
        };
        contentFrame = new Frame()
        {
            Name = "ContentPage",
            Background = Sprites.MenuBackground,
            Border = Sprites.MenuBorder,
            BorderThickness = Sprites.MenuBorderThickness,
            Margin = new(Top: -20),
        };
        // Spacer takes the place of left side-nav and keeps the main content window centered, not thrown off by the
        // scrollbar.
        var leftSpacer = new Panel() { Layout = LayoutParameters.FixedSize(48, 1) };
        scrollbar = new Scrollbar(
            Sprites.SmallUpArrow,
            Sprites.SmallDownArrow,
            Sprites.ScrollBarTrack,
            Sprites.VerticalScrollThumb)
        {
            Name = "ContentPageScroll",
            Layout = new()
            {
                Width = Length.Px(48),
                Height = Length.Stretch(),
            },
            Margin = new(Top: 100, Bottom: 20),
        };
        scrollingLayout = new Lane()
        {
            Name = "ScrollableFrameScrollingLayout",
            Layout = LayoutParameters.FitContent(),
            Children = [leftSpacer, contentFrame, scrollbar],
        };
        footerContainer = new Panel()
        {
            Name = "ScrollableFrameFooter",
            Layout = LayoutParameters.FitContent(),
            ZIndex = 1,
        };
        return new Panel()
        {
            Name = "ScrollableFrameContentLayout",
            Layout = LayoutParameters.FitContent(),
            HorizontalContentAlignment = Alignment.Middle,
            Children = [banner, scrollingLayout, footerContainer],
        };
    }
}
