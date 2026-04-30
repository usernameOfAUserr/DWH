namespace DWH.Domain.Entities.Loading;

/// <summary>
/// Enthält eine Eurostat-Beobachtung aus dem Datensatz jvs_q_nace2
/// zu offenen Stellen in der Branche Information und Kommunikation.
/// </summary>
public class EurostatJobVacancyData
{
    /// <summary>
    /// Quartal der Beobachtung, z. B. 2025-Q3.
    /// </summary>
    public string TimePeriod { get; set; } = string.Empty;

    /// <summary>
    /// Ländercode der Beobachtung, z. B. DE.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Ländername der Beobachtung, z. B. Germany.
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// NACE-Branchen-Code, z. B. J für Information and communication.
    /// </summary>
    public string IndustryCode { get; set; } = string.Empty;

    /// <summary>
    /// Bezeichnung der NACE-Branche.
    /// </summary>
    public string IndustryName { get; set; } = string.Empty;

    /// <summary>
    /// Eurostat-Indikatorcode, z. B. JOBVAC, JVR oder JVRCH_Q.
    /// </summary>
    public string IndicatorCode { get; set; } = string.Empty;

    /// <summary>
    /// Bezeichnung des Eurostat-Indikators.
    /// </summary>
    public string IndicatorName { get; set; } = string.Empty;

    /// <summary>
    /// Beobachteter Messwert des jeweiligen Indikators.
    /// </summary>
    public decimal ObservationValue { get; set; }

    /// <summary>
    /// Einheit des Messwerts, z. B. Anzahl oder Prozent.
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Saisonbereinigungsart der Beobachtung, z. B. SA.
    /// </summary>
    public string SeasonalAdjustment { get; set; } = string.Empty;

    /// <summary>
    /// Unternehmensgrößenklasse der Beobachtung, z. B. TOTAL.
    /// </summary>
    public string SizeClass { get; set; } = string.Empty;

    /// <summary>
    /// Eurostat-Statuscode der Beobachtung, z. B. p für provisional.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}