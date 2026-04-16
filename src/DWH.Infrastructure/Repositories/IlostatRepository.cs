using System.Globalization;
using System.Text.Json;
using DWH.Application.Interfaces.Repositories;
using DWH.Domain.Entities;

namespace DWH.Infrastructure.Repositories;

public class IlostatRepository(HttpClient httpClient) : IIlostatRepository
{
    private const string EmploymentStatisticUrl =
        "https://rplumber.ilo.org/data/indicator?id=EMP_TEMP_SEX_ECO_NB&classif1=ECO_ISIC4_J&sex=SEX_T&type=label&format=.json";

    public async Task<IReadOnlyCollection<IlostatInformationEmploymentData>> GetInformationAndComunicationEmploymentStatistic(
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, EmploymentStatisticUrl);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new InvalidOperationException(
                $"ILOSTAT API returned HTTP {(int)response.StatusCode} with empty body.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"ILOSTAT API returned HTTP {(int)response.StatusCode}. Content-Type: {contentType}. Body: {rawContent}");
        }

        using var document = JsonDocument.Parse(rawContent);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("ILOSTAT API response is not a JSON array.");
        }

        var items = new List<IlostatInformationEmploymentData>();

        foreach (var entry in root.EnumerateArray())
        {
            var item = new IlostatInformationEmploymentData
            {
                RefArea = GetString(entry, "ref_area"),
                AreaLabel = GetString(entry, "ref_area.label"),
                SourceLabel = GetString(entry, "source.label"),
                Total = GetDecimal(entry, "obs_value"),
                Time = GetString(entry, "time")
            };

            items.Add(item);
        }

        return items;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.ToString(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null => string.Empty,
            _ => property.ToString()
        };
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }

        var rawValue = property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return parsedValue;
        }

        return null;
    }
}