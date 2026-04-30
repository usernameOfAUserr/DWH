using System.Text.Json;
using System.Text.Json.Serialization;

namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// Dto einer AI-Exposure-API-Antwort
/// </summary>
/// <typeparam name="T">Datentyp der Elemente</typeparam>
public class AiExposureApiResponse<T>
{
    /// <summary>
    /// Datenliste
    /// </summary>
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];

    /// <summary>
    /// Metadaten
    /// </summary>
    [JsonPropertyName("meta")]
    public AiExposureMetaDto? Meta { get; set; }

    /// <summary>
    /// Rohjson der Antwort
    /// </summary>
    public JsonElement? RawJson { get; set; }
}