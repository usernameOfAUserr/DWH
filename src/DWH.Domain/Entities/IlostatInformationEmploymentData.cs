namespace DWH.Domain.Entities;

/// <summary>
/// Dto eines ILOSTAT-Beschäftigungsdatensatzes
/// </summary>
public class IlostatInformationEmploymentData
{
    /// <summary>
    /// Referenzgebiet
    /// </summary>
    public string RefArea { get; set; } = string.Empty;

    /// <summary>
    /// Gebietsbezeichnung
    /// </summary>
    public string AreaLabel { get; set; } = string.Empty;

    /// <summary>
    /// Quellenbezeichnung
    /// </summary>
    public string SourceLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gesamtwert
    /// </summary>
    public decimal? Total { get; set; }

    /// <summary>
    /// Zeit
    /// </summary>
    public string Time { get; set; } = string.Empty;
}