namespace DWH.Domain.Dtos.AiExposure;

/// <summary>
/// Dto eines OECD-Datenrequests
/// </summary>
public class AiExposureDataRequest
{
    /// <summary>
    /// Agency-Identifier, Pflicht
    /// </summary>
    public required string AgencyId { get; set; } = null!;

    /// <summary>
    /// Dataset-Identifier, Pflicht
    /// </summary>
    public required string DatasetId { get; set; } = null!;

    /// <summary>
    /// Dataset-Version
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Datenauswahl im SDMX-Format, z. B. all oder DEU...A
    /// </summary>
    public string? DataSelection { get; set; }

    /// <summary>
    /// Startperiode
    /// </summary>
    public string? StartPeriod { get; set; }

    /// <summary>
    /// Endperiode
    /// </summary>
    public string? EndPeriod { get; set; }

    /// <summary>
    /// Response-Format, z. B. jsondata, csvfile oder csvfilewithlabels
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// DimensionAtObservation
    /// </summary>
    public string? DimensionAtObservation { get; set; }

    /// <summary>
    /// Weitere optionale Query-Parameter
    /// </summary>
    public Dictionary<string, string>? AdditionalParameters { get; set; }
}