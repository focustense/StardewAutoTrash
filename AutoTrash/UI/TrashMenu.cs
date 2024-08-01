using AutoTrash2.Config;
using AutoTrash2.Data;
using StardewUI;
using StardewValley;

namespace AutoTrash2.UI;

internal class TrashMenu(Configuration config, TrashData data, GameLocation location) : ViewMenu<TrashablesView>
{
    protected override TrashablesView CreateView()
    {
        return new(config, data, location);
    }

    protected override string? FormatTooltip(IEnumerable<ViewChild> path)
    {
        var baseText = base.FormatTooltip(path);
        if (string.IsNullOrEmpty(baseText))
        {
            return null;
        }
        var isUsingGamepad = Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse;
        var (localButton, globalButton) = isUsingGamepad
            ? ("A", "X")
            : (I18n.Button_LeftClick(), I18n.Button_RightClick());
        return string.Join(
            Environment.NewLine,
            baseText + Environment.NewLine,
            I18n.TrashMenu_Item_Tooltip_Toggle_Local(localButton),
            I18n.TrashMenu_Item_Tooltip_Toggle_Global(globalButton));
    }
}
