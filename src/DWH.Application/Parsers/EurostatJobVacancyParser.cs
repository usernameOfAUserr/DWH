using System.Text.Json;
using DWH.Domain.Entities.Loading;

namespace DWH.Application.Parsers;

public static class EurostatJobVacancyParser
{
    public static IReadOnlyCollection<EurostatJobVacancyData> Parse(string rawJson)
    {
        using var document = JsonDocument.Parse(rawJson);
        var root = document.RootElement;

        var values = root.GetProperty("value");

        var statuses = root.TryGetProperty("status", out var statusElement)
            ? statusElement
            : default;

        var dimensions = root.GetProperty("dimension");

        var geoLabels = GetLabels(dimensions, "geo");
        var timeLabels = GetLabels(dimensions, "time");
        var indicatorLabels = GetLabels(dimensions, "indic_em");
        var naceLabels = GetLabels(dimensions, "nace_r2");
        var unitLabels = GetLabels(dimensions, "unit");
        var seasonalAdjustmentLabels = GetLabels(dimensions, "s_adj");
        var sizeClassLabels = GetLabels(dimensions, "sizeclas");

        var geoIndexes = GetIndexes(dimensions, "geo");
        var timeIndexes = GetIndexes(dimensions, "time");
        var indicatorIndexes = GetIndexes(dimensions, "indic_em");
        var naceIndexes = GetIndexes(dimensions, "nace_r2");
        var unitIndexes = GetIndexes(dimensions, "unit");
        var seasonalAdjustmentIndexes = GetIndexes(dimensions, "s_adj");
        var sizeClassIndexes = GetIndexes(dimensions, "sizeclas");

        var timeCodes = timeIndexes
            .OrderBy(x => x.Value)
            .Select(x => x.Key)
            .ToArray();

        var geoCode = geoIndexes.SingleOrDefault(x => x.Key == "DE").Key ?? "DE";
        var indicatorCode = indicatorIndexes.SingleOrDefault(x => x.Key == "JOBVAC").Key ?? "JOBVAC";
        var industryCode = naceIndexes.SingleOrDefault(x => x.Key == "J").Key ?? "J";

        var unitCode = unitIndexes.Count > 0
            ? unitIndexes.OrderBy(x => x.Value).First().Key
            : string.Empty;

        var seasonalAdjustmentCode = seasonalAdjustmentIndexes.Count > 0
            ? seasonalAdjustmentIndexes.OrderBy(x => x.Value).First().Key
            : string.Empty;

        var sizeClassCode = sizeClassIndexes.Count > 0
            ? sizeClassIndexes.OrderBy(x => x.Value).First().Key
            : string.Empty;

        var result = new List<EurostatJobVacancyData>();

        foreach (var valueProperty in values.EnumerateObject())
        {
            var flatIndex = int.Parse(valueProperty.Name);

            var timeIndex = flatIndex % timeCodes.Length;
            var timeCode = timeCodes[timeIndex];

            var status = string.Empty;

            if (statuses.ValueKind == JsonValueKind.Object &&
                statuses.TryGetProperty(valueProperty.Name, out var statusValue))
            {
                status = statusValue.GetString() ?? string.Empty;
            }

            result.Add(new EurostatJobVacancyData
            {
                TimePeriod = timeCode,
                CountryCode = geoCode,
                CountryName = geoLabels.GetValueOrDefault(geoCode, geoCode),
                IndustryCode = industryCode,
                IndustryName = naceLabels.GetValueOrDefault(industryCode, industryCode),
                IndicatorCode = indicatorCode,
                IndicatorName = indicatorLabels.GetValueOrDefault(indicatorCode, indicatorCode),
                Unit = unitLabels.GetValueOrDefault(unitCode, unitCode),
                SeasonalAdjustment = seasonalAdjustmentLabels.GetValueOrDefault(
                    seasonalAdjustmentCode,
                    seasonalAdjustmentCode),
                SizeClass = sizeClassLabels.GetValueOrDefault(sizeClassCode, sizeClassCode),
                ObservationValue = valueProperty.Value.GetDecimal(),
                Status = status
            });
        }

        return result
            .OrderBy(x => x.TimePeriod)
            .ToList();
    }

    private static Dictionary<string, int> GetIndexes(JsonElement dimensions, string dimensionName)
    {
        if (!dimensions.TryGetProperty(dimensionName, out var dimension))
        {
            return [];
        }

        if (!dimension.GetProperty("category").TryGetProperty("index", out var indexElement))
        {
            return [];
        }

        return indexElement
            .EnumerateObject()
            .ToDictionary(
                x => x.Name,
                x => x.Value.GetInt32());
    }

    private static Dictionary<string, string> GetLabels(JsonElement dimensions, string dimensionName)
    {
        if (!dimensions.TryGetProperty(dimensionName, out var dimension) ||
            !dimension.GetProperty("category").TryGetProperty("label", out var labelElement))
        {
            return [];
        }

        return labelElement
            .EnumerateObject()
            .ToDictionary(
                x => x.Name,
                x => x.Value.GetString() ?? x.Name);
    }
}