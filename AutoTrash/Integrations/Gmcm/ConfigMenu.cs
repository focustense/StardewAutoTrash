using AutoTrash2.Config;
using GenericModConfigMenu;
using StardewModdingAPI;

namespace AutoTrash2.Integrations.Gmcm;

internal static class ConfigMenu
{
    public static void Register(IManifest mod, Func<Configuration> config, Action reset, Action save)
    {
        if (Apis.Gmcm is not IGenericModConfigMenuApi gmcm)
        {
            return;
        }
        gmcm.Register(mod, reset, save);
        gmcm.AddSectionTitle(mod, I18n.Options_Controls_Heading);
        gmcm.AddKeybindList(
            mod,
            name: I18n.Options_MenuKey_Label,
            tooltip: I18n.Options_MenuKey_Tooltip,
            getValue: () => config().MenuKey,
            setValue: value => config().MenuKey = value);
        gmcm.AddKeybindList(
            mod,
            name: I18n.Options_ModifierKey_Label,
            tooltip: I18n.Options_ModifierKey_Tooltip,
            getValue: () => config().ModifierKey,
            setValue: value => config().ModifierKey = value);
        gmcm.AddSectionTitle(mod, I18n.Options_Gameplay_Heading);
        gmcm.AddBoolOption(
            mod,
            name: I18n.Options_SuppressPickupNotifications_Label,
            tooltip: I18n.Options_SuppressPickupNotifications_Tooltip,
            getValue: () => config().SuppressPickupNotification,
            setValue: value => config().SuppressPickupNotification = value);
        gmcm.AddBoolOption(
            mod,
            name: I18n.Options_TrashNotifications_Label,
            tooltip: () => I18n.Options_TrashNotifications_Tooltip(I18n.Options_SuppressPickupNotifications_Label()),
            getValue: () => config().EnableTrashNotification,
            setValue: value => config().EnableTrashNotification = value);
        gmcm.AddNumberOption(
            mod,
            name: I18n.Options_MinEmptySlots_Label,
            tooltip: I18n.Options_MinEmptySlots_Tooltip,
            getValue: () => config().MinEmptySlots,
            setValue: value => config().MinEmptySlots = value,
            min: 0,
            max: 12,
            formatValue: count => count > 0 ? count.ToString() : I18n.Options_MinEmptySlots_Disabled());
    }
}
