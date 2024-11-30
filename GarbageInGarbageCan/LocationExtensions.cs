using System.Text;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace AutoTrash2;

/// <summary>
/// Extension methods for the <see cref="GameLocation"/> type.
/// </summary>
internal static class LocationExtensions
{
    private const string NAME_SEPARATOR = " > ";

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
                uniqueName =
                    uniqueName[..^mineLevelText.Length] + "_" + mineShaft.locationContextId;
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
        if (location.ParentBuilding is { } building && building.GetData() is { } buildingData)
        {
            if (building.GetParentLocation() is { } buildingParentLocation)
            {
                AppendLocationText(buildingParentLocation, sb);
            }
            AppendSeparatorIfNonEmpty(sb);
            sb.Append(TokenParser.ParseText(buildingData.Name));
        }
        else if (location.GetParentLocation() is { } parentLocation)
        {
            AppendLocationText(parentLocation, sb);
        }

        // GameLocation.DisplayName automatically falls back to the parent DisplayName if this location doesn't have its
        // own. But we don't want this, as it will get duplicated, so we use the raw location data instead.
        if (
            location.GetData() is { } locationData
            && !string.IsNullOrEmpty(locationData.DisplayName)
        )
        {
            AppendSeparatorIfNonEmpty(sb);
            sb.Append(TokenParser.ParseText(locationData.DisplayName));
        }
    }

    private static void AppendSeparatorIfNonEmpty(StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            sb.Append(NAME_SEPARATOR);
        }
    }
}
