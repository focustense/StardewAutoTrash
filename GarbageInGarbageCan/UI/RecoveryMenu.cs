using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace AutoTrash2.UI;

internal class RecoveryMenu(IList<Item> inventory, Action<Item> onRecoverItem)
    : ItemGrabMenu(
        inventory,
        reverseGrab: false,
        showReceivingMenu: true,
        highlightFunction: InventoryMenu.highlightAllItems,
        behaviorOnItemSelectFunction: (item, _) => item.SetTrashCheckBypass(true),
        behaviorOnItemGrab: (item, _) =>
        {
            item.SetTrashCheckBypass(true);
            onRecoverItem(item);
        },
        message: "",
        canBeExitedWithKey: true
    )
{
    public override void draw(SpriteBatch b)
    {
        // HACK: This is probably not the best way to force the actual "ItemGrabMenu.draw" to be called, but if we just
        // call `base.draw(b)` directly, the compiler gets confused and tries to call the `MenuWithInventory.draw`
        // method instead, which has `SpriteBatch` + a bunch of other optional args.
        Action<SpriteBatch> baseDraw = base.draw;
        baseDraw.Invoke(b);
        var title = I18n.RecoveryMenu_Title();
        var yOffset = SpriteText.getHeightOfString(title) + 80;
        SpriteText.drawStringWithScrollCenteredAt(
            b,
            title,
            xPositionOnScreen + width / 2,
            yPositionOnScreen - yOffset
        );
    }
}
