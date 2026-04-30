namespace DWH.Domain.Entities;

/// <summary>
/// Entität eines Destatis-Datensatzes zu sozialversicherungspflichtig Beschäftigten
/// </summary>
public class GermanEmploymentData
{
    /// <summary>
    /// Stichtag des Datensatzes
    /// </summary>
    public required DateTime ReferenceDate { get; set; }

    /// <summary>
    /// Jahr des Datensatzes
    /// </summary>
    public required int Year { get; set; }

    /// <summary>
    /// Code des Wirtschaftsabschnitts
    /// </summary>
    public required string EconomicSectionCode { get; set; }

    /// <summary>
    /// Bezeichnung des Wirtschaftsabschnitts
    /// </summary>
    public required string EconomicSection { get; set; }

    /// <summary>
    /// Anzahl der sozialversicherungspflichtig Beschäftigten
    /// </summary>
    public required int EmploymentCount { get; set; }
}