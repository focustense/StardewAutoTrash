using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;
using StardewValley.TokenizableStrings;
using System.Text;

namespace AutoTrash2;

/// <summary>
/// Extension methods for the <see cref="GameLocation"/> type.
/// </summary>
internal static class LocationExtensions
{
    private const string NameSeparator = " > ";

    public static string GetPathText(this GameLocation location)
    {
        var sb = new StringBuilder();
        AppendLocationText(location, sb);
        return sb.ToString();
    }

    private static void AppendLocationText(GameLocation location, StringBuilder sb)
    {
        if (location.GetContainingBuilding() is Building building
            && building.GetData() is BuildingData buildingData)
        {
            if (building.GetParentLocation() is GameLocation buildingParentLocation)
            {
                AppendLocationText(buildingParentLocation, sb);
            }
            AppendSeparatorIfNonEmpty(sb);
            sb.Append(TokenParser.ParseText(buildingData.Name));
        }
        else if (location.GetParentLocation() is GameLocation parentLocation)
        {
            AppendLocationText(parentLocation, sb);
        }

        // GameLocation.DisplayName automatically falls back to the parent DisplayName if this location doesn't have its
        // own. But we don't want this, as it will get duplicated, so we use the raw location data instead.
        if (location.GetData() is LocationData locationData
            && !string.IsNullOrEmpty(locationData.DisplayName))
        {
            AppendSeparatorIfNonEmpty(sb);
            sb.Append(TokenParser.ParseText(locationData.DisplayName));
        }
    }

    private static void AppendSeparatorIfNonEmpty(StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            sb.Append(NameSeparator);
        }
    }
}
