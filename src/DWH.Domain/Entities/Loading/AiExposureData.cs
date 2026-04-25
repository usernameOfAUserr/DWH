namespace DWH.Domain.Entities.Loading;

public class AiExposureData
{
    public required string Occupation { get; set; }

    public required decimal AiExposureScore { get; set; }

    public required int Employment { get; set; }

    public required decimal MedianWage { get; set; }

    public required decimal ProjectedGrowthPct { get; set; }

    public required string TopRiskFactors { get; set; }

    public required decimal RiskScore { get; set; }

    public required string TopSafeTasks { get; set; }
}