using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AutoTrash2.Config;

/// <summary>
/// Mod configuration settings.
/// </summary>
internal class Configuration
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
}
