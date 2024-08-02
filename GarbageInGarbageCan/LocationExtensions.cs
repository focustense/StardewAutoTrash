using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;
using System.Text;

namespace AutoTrash2;

/// <summary>
/// Extension methods for the <see cref="GameLocation"/> type.
/// </summary>
internal static class LocationExtensions
{
    private const string NameSeparator = " > ";

    /// <summary>
    /// Formats the full hierarchical path of a location, e.g. "Johnson Farm > Farmhouse".
    /// </summary>
    public static string GetPathText(this GameLocation location)
    {
        var sb = new StringBuilder();
        AppendLocationText(location, sb);
        var result = sb.ToString();
        return !string.IsNullOrEmpty(result) ? result : GetSemiUniqueKey(location);
    }

    /// <summary>
    /// Gets a "semi-unique" key for a location, e.g. that defines a broad "area" such as Mines or Skull Cavern but not
    /// the specific floor.
    /// </summary>
    public static string GetSemiUniqueKey(this GameLocation location)
    {
        var uniqueName = location.NameOrUniqueName;
        if (location is MineShaft mineShaft)
        {
            var mineLevelText = mineShaft.mineLevel.ToString();
            if (uniqueName.EndsWith(mineLevelText))
            {
                uniqueName = uniqueName[..^mineLevelText.Length] + "_" + mineShaft.locationContextId;
            }
        }
        else if (location is VolcanoDungeon volcano)
        {
            var volcanoLevelText = volcano.level.Value.ToString();
            if (uniqueName.EndsWith(volcanoLevelText))
            {
                uniqueName = uniqueName[..^volcanoLevelText.Length];
            }
        }
        return uniqueName;
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
