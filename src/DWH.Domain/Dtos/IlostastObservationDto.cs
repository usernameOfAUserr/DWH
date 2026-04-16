namespace DWH.Domain.Dtos;

public class IlostatObservationDto
{
    public Dictionary<string, string> Dimensions { get; set; } = [];

    public string? Value { get; set; }
}