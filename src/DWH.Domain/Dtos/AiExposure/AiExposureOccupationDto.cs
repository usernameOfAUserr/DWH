using System.Text.Json.Serialization;

namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// DTO eines Berufs aus der AI-Exposure-API.
/// </summary>
public class AiExposureOccupationDto
{
    [JsonPropertyName("slug")] public string? Slug { get; set; }

    [JsonPropertyName("socCode")] public string? SocCode { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonPropertyName("category")] public string? Category { get; set; }

    [JsonPropertyName("riskScore")] public decimal? RiskScore { get; set; }

    [JsonPropertyName("freyOsborneProb")] public decimal? FreyOsborneProb { get; set; }

    [JsonPropertyName("employment")] public int? Employment { get; set; }

    [JsonPropertyName("medianWage")] public decimal? MedianWage { get; set; }

    [JsonPropertyName("wage10th")] public decimal? Wage10th { get; set; }

    [JsonPropertyName("wage90th")] public decimal? Wage90th { get; set; }

    [JsonPropertyName("projectedGrowthPct")]
    public decimal? ProjectedGrowthPct { get; set; }

    [JsonPropertyName("genaiExposure")] public decimal? GenAiExposure { get; set; }

    [JsonPropertyName("topRiskFactors")] public List<string>? TopRiskFactors { get; set; }

    [JsonPropertyName("topSafeTasks")] public List<string>? TopSafeTasks { get; set; }

    [JsonPropertyName("topTasks")] public List<string>? TopTasks { get; set; }

    [JsonPropertyName("topSkills")] public List<string>? TopSkills { get; set; }

    [JsonPropertyName("topKnowledge")] public List<string>? TopKnowledge { get; set; }
}