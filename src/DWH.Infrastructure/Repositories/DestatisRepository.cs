using System.Text.Json;
using DWH.Application.Helpers;
using DWH.Application.Interfaces.Repositories;
using DWH.Domain.Dtos.Destatis;
using DWH.Domain.Models.DataSources;
using Microsoft.Extensions.Options;

namespace DWH.Infrastructure.Repositories;

public class DestatisRepository(HttpClient httpClient, IOptions<DestatisOptions> options) : IDestatisRepository
{
    private const string BaseUrl = "https://www-genesis.destatis.de/genesisWS/rest/2020";
    
    private readonly DestatisOptions _options = options.Value;
    
    public async Task<DestatisApiResponse> ListAsync(DestatisTableRequest request,
        CancellationToken cancellationToken = default)
    {
        return await GetTableJsonAsync(request, cancellationToken);
    }

    private async Task<DestatisApiResponse> GetTableJsonAsync(DestatisTableRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/data/table");
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        
        _options.Password = Environment.GetEnvironmentVariable("DESTATIS_PASSWORD") ?? throw new
            InvalidOperationException();
        
        var hasCredentials =
            !string.IsNullOrWhiteSpace(_options.Username) &&
            !string.IsNullOrWhiteSpace(_options.Password);

        if (!hasCredentials)
        {
            throw new InvalidOperationException("Destatis credentials are missing.");
        }

        httpRequest.Headers.TryAddWithoutValidation("username", _options.Username);
        httpRequest.Headers.TryAddWithoutValidation("password", _options.Password);

        var parameters = BuildTableBodyParameters(request)
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Value!))
            .ToList();

        httpRequest.Content = new FormUrlEncodedContent(parameters);

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new InvalidOperationException(
                $"Destatis API returned HTTP {(int)response.StatusCode} with empty body.");
        }

        if (rawContent.TrimStart().StartsWith("<", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Destatis API returned HTML instead of JSON. HTTP {(int)response.StatusCode}. Content-Type: {contentType}. Body: {rawContent}");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Destatis API returned HTTP {(int)response.StatusCode}. Content-Type: {contentType}. Body: {rawContent}");
        }

        using var document = JsonDocument.Parse(rawContent);
        var root = document.RootElement.Clone();

        var result = MapApiResponse(root);

        if (!string.IsNullOrWhiteSpace(result.Status?.Code) && result.Status.Code != "0")
        {
            throw new InvalidOperationException(
                $"Destatis table request failed: {result.Status.Content ?? result.Status.Code}");
        }

        return result;
    }

    private Dictionary<string, string?> BuildTableBodyParameters(DestatisTableRequest request)
    {
        return new Dictionary<string, string?>
        {
            ["name"] = request.Name,
            ["area"] = string.IsNullOrWhiteSpace(request.Area) ? _options.Area : request.Area,
            ["compress"] = (request.Compress ?? _options.Compress).ToString().ToLowerInvariant(),
            ["transpose"] = (request.Transpose ?? _options.Transpose).ToString().ToLowerInvariant(),
            ["contents"] = request.Contents,
            ["startyear"] = request.StartYear?.ToString(),
            ["endyear"] = request.EndYear?.ToString(),
            ["timeslices"] = request.TimeSlices,
            ["regionalvariable"] = request.RegionalVariable,
            ["regionalkey"] = request.RegionalKey is { Length: > 0 } ? string.Join(",", request.RegionalKey) : null,
            ["classifyingvariable1"] = request.ClassifyingVariable1,
            ["classifyingkey1"] = request.ClassifyingKey1 is { Length: > 0 }
                ? string.Join(",", request.ClassifyingKey1)
                : null,
            ["classifyingvariable2"] = request.ClassifyingVariable2,
            ["classifyingkey2"] = request.ClassifyingKey2 is { Length: > 0 }
                ? string.Join(",", request.ClassifyingKey2)
                : null,
            ["classifyingvariable3"] = request.ClassifyingVariable3,
            ["classifyingkey3"] = request.ClassifyingKey3 is { Length: > 0 }
                ? string.Join(",", request.ClassifyingKey3)
                : null,
            ["classifyingvariable4"] = request.ClassifyingVariable4,
            ["classifyingkey4"] = request.ClassifyingKey4 is { Length: > 0 }
                ? string.Join(",", request.ClassifyingKey4)
                : null,
            ["classifyingvariable5"] = request.ClassifyingVariable5,
            ["classifyingkey5"] = request.ClassifyingKey5 is { Length: > 0 }
                ? string.Join(",", request.ClassifyingKey5)
                : null,
            ["job"] = (request.Job ?? false).ToString().ToLowerInvariant(),
            ["stand"] = request.Stand,
            ["language"] = string.IsNullOrWhiteSpace(request.Language) ? _options.Language : request.Language
        };
    }

    private static DestatisApiResponse MapApiResponse(JsonElement root)
    {
        return new DestatisApiResponse
        {
            RawJson = root.Clone(),
            Status = TryMapStatus(root)
        };
    }

    private static DestatisStatusDto? TryMapStatus(JsonElement root)
    {
        if (!TryGetProperty(root, "Status", out var statusElement) &&
            !TryGetProperty(root, "status", out statusElement))
        {
            return null;
        }

        if (statusElement.ValueKind == JsonValueKind.String)
        {
            var value = statusElement.GetString();

            return new DestatisStatusDto
            {
                Code = value,
                Content = value,
                Type = null,
                Raw = statusElement.Clone()
            };
        }

        if (statusElement.ValueKind != JsonValueKind.Object)
        {
            return new DestatisStatusDto
            {
                Code = null,
                Content = statusElement.ToString(),
                Type = null,
                Raw = statusElement.Clone()
            };
        }

        return new DestatisStatusDto
        {
            Code = TryGetString(statusElement, "code") ?? TryGetString(statusElement, "Code"),
            Content = TryGetString(statusElement, "content") ?? TryGetString(statusElement, "Content"),
            Type = TryGetString(statusElement, "type") ?? TryGetString(statusElement, "Type"),
            Raw = statusElement.Clone()
        };
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => value.ToString()
        };
    }
}