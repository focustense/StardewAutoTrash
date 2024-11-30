using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AutoTrash2.Config;

/// <summary>
/// Mod configuration settings.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Whether to display a notification when items are automatically trashed.
    /// </summary>
    public bool EnableTrashNotification { get; set; } = true;

    /// <summary>
    /// Whether to suppress the normal notification when an auto-trashed item is initially picked up.
    /// </summary>
    public bool SuppressPickupNotification { get; set; } = true;

    /// <summary>
    /// Hotkey for bringing up the trash menu.
    /// </summary>
    public KeybindList MenuKey { get; set; } = new(SButton.G);

    /// <summary>
    /// Key to hold down when trashing an item for the first time to make it auto-trashable.
    /// </summary>
    public KeybindList ModifierKey { get; set; } = new(SButton.LeftControl);

    /// <summary>
    /// Minimum number of empty backpack slots to try to maintain by trashing; 0 = disabled.
    /// </summary>
    /// <remarks>
    /// If enabled (non-zero), items will not be trashed until either (a) the number of empty slots dips below this
    /// number due to a <em>non-trash</em> item being picked up, which will cause trash items already in the inventory
    /// to be discarded, or (b) the number of empty slots is exactly equal to this number and the player is about to
    /// pick up a trash item that does not stack with any existing inventory; that is, if the number of empty slots
    /// <em>would</em> drop below the minimum if the item were to be accepted.
    /// </remarks>
    public int MinEmptySlots { get; set; } = 0;

    /// <summary>
    /// Time limit during which a recently-trashed item can be recovered, after which it is lost permanently.
    /// </summary>
    public TimeSpan RecoveryLimit { get; set; } = TimeSpan.FromSeconds(5);
}
