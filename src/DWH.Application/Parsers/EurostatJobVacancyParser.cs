using System.Text.Json;
using DWH.Domain.Entities.Loading;

namespace DWH.Application.Parsers;

public static class EurostatJobVacancyParser
{
    public static IReadOnlyCollection<EurostatJobVacancyData> Parse(string rawJson)
    {
        using var document = JsonDocument.Parse(rawJson);
        var root = document.RootElement;

        if (!root.TryGetProperty("value", out var values))
        {
            return [];
        }

        var statuses = root.TryGetProperty("status", out var statusElement)
            ? statusElement
            : default;

        var dimensions = root.GetProperty("dimension");
        var dimensionOrder = root.GetProperty("id")
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var sizes = root.GetProperty("size")
            .EnumerateArray()
            .Select(x => x.GetInt32())
            .ToArray();

        var geoLabels = GetLabels(dimensions, "geo");
        var indicatorLabels = GetLabels(dimensions, "indic_em");
        var naceLabels = GetLabels(dimensions, "nace_r2");
        var unitLabels = GetLabels(dimensions, "unit");

        var dimensionCodesByName = dimensionOrder.ToDictionary(
            dimensionName => dimensionName,
            dimensionName => GetCodesOrderedByIndex(dimensions, dimensionName));

        var result = new List<EurostatJobVacancyData>();

        foreach (var valueProperty in values.EnumerateObject())
        {
            var flatIndex = int.Parse(valueProperty.Name);
            var coordinates = GetCoordinates(flatIndex, sizes);

            var codes = new Dictionary<string, string>();

            for (var i = 0; i < dimensionOrder.Length; i++)
            {
                var dimensionName = dimensionOrder[i];
                var dimensionCodes = dimensionCodesByName[dimensionName];
                var coordinate = coordinates[i];

                if (coordinate >= 0 && coordinate < dimensionCodes.Length)
                {
                    codes[dimensionName] = dimensionCodes[coordinate];
                }
            }

            var timePeriod = GetCode(codes, "time");
            var geoCode = GetCode(codes, "geo");
            var industryCode = GetCode(codes, "nace_r2");
            var indicatorCode = GetCode(codes, "indic_em");
            var unitCode = GetCode(codes, "unit");
            var seasonalAdjustmentCode = GetCode(codes, "s_adj");
            var sizeClassCode = GetCode(codes, "sizeclas");

            var status = string.Empty;

            if (statuses.ValueKind == JsonValueKind.Object &&
                statuses.TryGetProperty(valueProperty.Name, out var statusValue))
            {
                status = statusValue.GetString() ?? string.Empty;
            }

            result.Add(new EurostatJobVacancyData
            {
                TimePeriod = timePeriod,

                CountryCode = geoCode,
                CountryName = geoLabels.GetValueOrDefault(geoCode, geoCode),

                IndustryCode = industryCode,
                IndustryName = naceLabels.GetValueOrDefault(industryCode, industryCode),

                IndicatorCode = indicatorCode,
                IndicatorName = indicatorLabels.GetValueOrDefault(indicatorCode, indicatorCode),

                Unit = unitLabels.GetValueOrDefault(unitCode, unitCode),
                SeasonalAdjustment = seasonalAdjustmentCode,
                SizeClass = sizeClassCode,

                ObservationValue = valueProperty.Value.GetDecimal(),
                Status = status
            });
        }

        return result
            .OrderBy(x => x.CountryCode)
            .ThenBy(x => x.TimePeriod)
            .ToList();
    }

    private static int[] GetCoordinates(int flatIndex, int[] sizes)
    {
        var coordinates = new int[sizes.Length];

        for (var i = sizes.Length - 1; i >= 0; i--)
        {
            coordinates[i] = flatIndex % sizes[i];
            flatIndex /= sizes[i];
        }

        return coordinates;
    }

    private static string GetCode(Dictionary<string, string> codes, string dimensionName)
    {
        return codes.GetValueOrDefault(dimensionName, string.Empty);
    }

    private static string[] GetCodesOrderedByIndex(JsonElement dimensions, string dimensionName)
    {
        var indexes = GetIndexes(dimensions, dimensionName);

        return indexes
            .OrderBy(x => x.Value)
            .Select(x => x.Key)
            .ToArray();
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