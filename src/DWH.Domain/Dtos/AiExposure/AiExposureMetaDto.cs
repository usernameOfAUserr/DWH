using System.Text.Json.Serialization;

namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// Dto der Metadaten einer AI-Exposure-API-Antwort
/// </summary>
public class AiExposureMetaDto
{
    /// <summary>
    /// Gesamtanzahl
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// Limit
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Offset
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    /// <summary>
    /// Sortierung
    /// </summary>
    [JsonPropertyName("sort")]
    public string? Sort { get; set; }

    /// <summary>
    /// Reihenfolge
    /// </summary>
    [JsonPropertyName("order")]
    public string? Order { get; set; }
}