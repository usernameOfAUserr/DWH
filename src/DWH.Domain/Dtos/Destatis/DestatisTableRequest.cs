namespace DWH.Domain.Dtos.Destatis;

/// <summary>
/// Dto eines technischen Destatis-Tabellenrequests
/// </summary>
public sealed record DestatisTableRequest(
    string Name,
    string? Area = null,
    bool? Compress = null,
    bool? Transpose = null,
    string? Contents = null,
    int? StartYear = null,
    int? EndYear = null,
    string? TimeSlices = null,
    string? RegionalVariable = null,
    string[]? RegionalKey = null,
    string? ClassifyingVariable1 = null,
    string[]? ClassifyingKey1 = null,
    string? ClassifyingVariable2 = null,
    string[]? ClassifyingKey2 = null,
    string? ClassifyingVariable3 = null,
    string[]? ClassifyingKey3 = null,
    string? ClassifyingVariable4 = null,
    string[]? ClassifyingKey4 = null,
    string? ClassifyingVariable5 = null,
    string[]? ClassifyingKey5 = null,
    string? Stand = null,
    bool? Job = null,
    string? Language = null
);