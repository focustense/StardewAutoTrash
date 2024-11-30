using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace AutoTrash2.UI;

/// <summary>
/// Tooltip data for an item listed in the trashable items menu.
/// </summary>
/// <param name="Title">The tooltip's title line, i.e. name of the item.</param>
/// <param name="Text">The main text to display. This is usually <em>not</em> the item description, but instead an input
/// (mouse or gamepad) button prompt explaining the available actions.</param>
/// <param name="CurrencyAmount">Sell value of the item.</param>
public record TrashItemTooltip(string Title, string Text, int? CurrencyAmount)
{
    /// <summary>
    /// Creates tooltip data from an item's details.
    /// </summary>
    /// <param name="qualifiedItemId">Qualified ID for the item being displayed.</param>
    /// <param name="playerId">Unique identifier of the player opening the menu. Affects displayed sell values.</param>
    /// <param name="gamepadControls">Whether the current input device is a gamepad. Affects the button prompts
    /// displayed in the main body.</param>
    public static TrashItemTooltip ForItem(
        string qualifiedItemId,
        long playerId,
        bool gamepadControls
    )
    {
        var item = ItemRegistry.Create(qualifiedItemId);
        string instructions = FormatInstructions(gamepadControls);
        int? salePrice =
            item is not Clothing
            && item is not Furniture
            && (item is not SObject obj || !obj.bigCraftable.Value)
                ? item.sellToStorePrice(playerId)
                : null;
        return new(item.DisplayName, instructions, salePrice);
    }

    private static string FormatInstructions(bool gamepadControls)
    {
        var (localButton, globalButton) = gamepadControls
            ? ("A", "X")
            : (I18n.Button_LeftClick(), I18n.Button_RightClick());
        return string.Join(
            Environment.NewLine,
            I18n.TrashMenu_Item_Tooltip_Toggle_Local(localButton),
            I18n.TrashMenu_Item_Tooltip_Toggle_Global(globalButton)
        );
    }
}
