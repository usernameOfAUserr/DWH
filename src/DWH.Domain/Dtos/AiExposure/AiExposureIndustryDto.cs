using System.Text.Json.Serialization;

namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// Dto einer Branche aus AI Exposure
/// </summary>
public class AiExposureIndustryDto
{
    /// <summary>
    /// Slug der Branche
    /// </summary>
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    /// <summary>
    /// Name der Branche
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// NAICS-Code
    /// </summary>
    [JsonPropertyName("naicsCode")]
    public string? NaicsCode { get; set; }

    /// <summary>
    /// Durchschnittlicher Risiko-Score
    /// </summary>
    [JsonPropertyName("avgRiskScore")]
    public decimal? AvgRiskScore { get; set; }

    /// <summary>
    /// Gesamtbeschäftigung
    /// </summary>
    [JsonPropertyName("totalEmployment")]
    public int? TotalEmployment { get; set; }

    /// <summary>
    /// Medianlohn
    /// </summary>
    [JsonPropertyName("medianWage")]
    public decimal? MedianWage { get; set; }
}