using System.Text.Json;
using DWH.Application.Interfaces.Repositories;
using DWH.Domain.Dtos.AiExposure;
using Microsoft.AspNetCore.WebUtilities;

namespace DWH.Infrastructure.Repositories;

public class AiExposureRepository(HttpClient httpClient) : IAiExposureRepository
{
    private const string BaseUrl = "https://www.aiexposure.org/api/v1";
    private const int DefaultPageSize = 100;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AiExposureApiResponse<AiExposureOccupationDto>> GetOccupationsAsync(
        CancellationToken cancellationToken = default)
    {
        var allItems = new List<AiExposureOccupationDto>();
        var offset = 0;
        AiExposureMetaDto? lastMeta = null;
        JsonElement? lastRawJson = null;

        while (true)
        {
            var url = BuildPagedUrl(
                "occupations",
                "title",
                "asc",
                DefaultPageSize,
                offset);

            var page = await GetWrappedAsync<AiExposureOccupationDto>(url, cancellationToken);

            if (page.Data.Count == 0)
            {
                break;
            }

            allItems.AddRange(page.Data);
            lastMeta = page.Meta;
            lastRawJson = page.RawJson;

            var loadedCount = offset + page.Data.Count;
            var total = page.Meta?.Total;

            if (total.HasValue && loadedCount >= total.Value)
            {
                break;
            }

            if (page.Data.Count < DefaultPageSize)
            {
                break;
            }

            offset += DefaultPageSize;
        }

        return new AiExposureApiResponse<AiExposureOccupationDto>
        {
            Data = allItems,
            Meta = new AiExposureMetaDto
            {
                Total = allItems.Count,
                Limit = lastMeta?.Limit ?? DefaultPageSize,
                Offset = 0,
                Sort = "title",
                Order = "asc"
            },
            RawJson = lastRawJson
        };
    }

    private async Task<AiExposureApiResponse<T>> GetWrappedAsync<T>(
        string url,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new InvalidOperationException(
                $"AI Exposure API returned HTTP {(int)response.StatusCode} with empty body.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"AI Exposure API returned HTTP {(int)response.StatusCode}. Content-Type: {contentType}. Body: {rawContent}");
        }

        var result = JsonSerializer.Deserialize<AiExposureApiResponse<T>>(rawContent, JsonSerializerOptions);

        if (result is null)
        {
            throw new InvalidOperationException("AI Exposure API response could not be deserialized.");
        }

        using var document = JsonDocument.Parse(rawContent);
        result.RawJson = document.RootElement.Clone();

        return result;
    }

    private static string BuildPagedUrl(
        string endpoint,
        string sort,
        string order,
        int limit,
        int offset)
    {
        var baseUrl = $"{BaseUrl}/{endpoint}";

        var query = new Dictionary<string, string?>
        {
            ["sort"] = sort,
            ["order"] = order,
            ["limit"] = limit.ToString(),
            ["offset"] = offset.ToString()
        };

        return QueryHelpers.AddQueryString(baseUrl, query);
    }
}