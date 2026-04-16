namespace DWH.Domain.Entities;

public class AiExposureData
{
    public required string Occupation { get; set; }

    public required decimal AiExposureScore { get; set; }
}