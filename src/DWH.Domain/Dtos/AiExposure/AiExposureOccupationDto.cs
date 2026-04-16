using System.Text.Json.Serialization;

namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// Dto eines Berufs aus AI Exposure
/// </summary>
public class AiExposureOccupationDto
{
    /// <summary>
    /// Slug des Berufs
    /// </summary>
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    /// <summary>
    /// Titel des Berufs
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// SOC-Code
    /// </summary>
    [JsonPropertyName("socCode")]
    public string? SocCode { get; set; }

    /// <summary>
    /// Risiko-Score
    /// </summary>
    [JsonPropertyName("riskScore")]
    public decimal? RiskScore { get; set; }

    /// <summary>
    /// Beschäftigung
    /// </summary>
    [JsonPropertyName("employment")]
    public int? Employment { get; set; }

    /// <summary>
    /// Medianlohn
    /// </summary>
    [JsonPropertyName("medianWage")]
    public decimal? MedianWage { get; set; }

    /// <summary>
    /// GenAI-Exposition
    /// </summary>
    [JsonPropertyName("genAiExposure")]
    public decimal? GenAiExposure { get; set; }

    /// <summary>
    /// Prognostiziertes Wachstum
    /// </summary>
    [JsonPropertyName("projectedGrowth")]
    public decimal? ProjectedGrowth { get; set; }

    /// <summary>
    /// Frey-Osborne-Wahrscheinlichkeit
    /// </summary>
    [JsonPropertyName("freyOsborneProbability")]
    public decimal? FreyOsborneProbability { get; set; }

    /// <summary>
    /// Risikostufe
    /// </summary>
    [JsonPropertyName("riskLevel")]
    public string? RiskLevel { get; set; }
}