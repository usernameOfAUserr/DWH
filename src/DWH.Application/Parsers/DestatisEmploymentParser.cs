using System.Globalization;
using System.Text.Json;
using DWH.Domain.Entities;

namespace DWH.Application.Parsers;

/// <summary>
/// Parser für Destatis-Beschäftigungsdaten aus der Tabelle 13111-0004
/// </summary>
public static class DestatisEmploymentParser
{
    /// <summary>
    /// Parst die vollständige Destatis-API-Antwort in Beschäftigungsdatensätze
    /// </summary>
    /// <param name="json">Vollständige JSON-Antwort der Destatis-API</param>
    /// <param name="onlyInformationAndCommunication">Gibt an, ob nur WZ08-J importiert werden soll</param>
    /// <param name="includeUnknownEmploymentType">Gibt an, ob Zeilen mit Beschäftigungsumfang "Ohne Angabe" berücksichtigt werden sollen</param>
    /// <returns>Geparste Beschäftigungsdatensätze</returns>
    public static IReadOnlyCollection<GermanEmploymentData> ParseEmploymentResponse(
        string json,
        bool onlyInformationAndCommunication = false,
        bool includeUnknownEmploymentType = false)
    {
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("Object", out var objectElement))
        {
            throw new InvalidOperationException("Die API-Antwort enthält kein 'Object'.");
        }

        if (!objectElement.TryGetProperty("Content", out var contentElement))
        {
            throw new InvalidOperationException("Die API-Antwort enthält kein 'Object.Content'.");
        }

        var content = contentElement.GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<GermanEmploymentData>();
        }

        return ParseEmploymentContent(content, onlyInformationAndCommunication, includeUnknownEmploymentType);
    }

    /// <summary>
    /// Parst den CSV-Inhalt aus Object.Content in Beschäftigungsdatensätze
    /// </summary>
    /// <param name="content">CSV-Inhalt aus der Destatis-Antwort</param>
    /// <param name="onlyInformationAndCommunication">Gibt an, ob nur WZ08-J importiert werden soll</param>
    /// <param name="includeUnknownEmploymentType">Gibt an, ob Zeilen mit Beschäftigungsumfang "Ohne Angabe" berücksichtigt werden sollen</param>
    /// <returns>Geparste Beschäftigungsdatensätze</returns>
    public static IReadOnlyCollection<GermanEmploymentData> ParseEmploymentContent(
        string content,
        bool onlyInformationAndCommunication = false,
        bool includeUnknownEmploymentType = false)
    {
        var lines = content
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();

        if (lines.Count == 0)
        {
            return Array.Empty<GermanEmploymentData>();
        }

        var dateHeaderLine = lines.FirstOrDefault(IsDateHeaderLine);

        if (string.IsNullOrWhiteSpace(dateHeaderLine))
        {
            throw new InvalidOperationException("Es konnte keine Stichtags-Header-Zeile gefunden werden.");
        }

        var referenceDates = ParseReferenceDates(dateHeaderLine);

        if (referenceDates.Count == 0)
        {
            throw new InvalidOperationException("Es konnten keine Stichtage geparst werden.");
        }

        var aggregated =
            new Dictionary<(DateTime ReferenceDate, string EconomicSectionCode, string EconomicSection), int>();

        foreach (var line in lines)
        {
            if (!IsDataLine(line))
            {
                continue;
            }

            var parts = line.Split(';');

            if (parts.Length < 4 + referenceDates.Count)
            {
                continue;
            }

            var economicSectionCode = parts[0].Trim();
            var economicSection = parts[1].Trim();
            var employmentType = parts[2].Trim();
            var gender = parts[3].Trim();

            if (string.IsNullOrWhiteSpace(economicSectionCode) ||
                string.IsNullOrWhiteSpace(economicSection))
            {
                continue;
            }

            if (string.Equals(economicSectionCode, "OA", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (onlyInformationAndCommunication &&
                !string.Equals(economicSectionCode, "WZ08-J", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(gender, "Insgesamt", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!includeUnknownEmploymentType &&
                string.Equals(employmentType, "Ohne Angabe", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            for (var i = 0; i < referenceDates.Count; i++)
            {
                var valueIndex = i + 4;

                if (valueIndex >= parts.Length)
                {
                    continue;
                }

                var rawValue = parts[valueIndex].Trim();

                if (!TryParseEmploymentValue(rawValue, out var employmentCount))
                {
                    continue;
                }

                var key = (referenceDates[i], economicSectionCode, economicSection);

                if (!aggregated.TryAdd(key, employmentCount))
                {
                    aggregated[key] += employmentCount;
                }
            }
        }

        return aggregated
            .OrderBy(x => x.Key.ReferenceDate)
            .ThenBy(x => x.Key.EconomicSectionCode)
            .Select(x => new GermanEmploymentData
            {
                ReferenceDate = x.Key.ReferenceDate,
                Year = x.Key.ReferenceDate.Year,
                EconomicSectionCode = x.Key.EconomicSectionCode,
                EconomicSection = x.Key.EconomicSection,
                EmploymentCount = x.Value
            })
            .ToList();
    }

    /// <summary>
    /// Gibt an, ob eine Zeile die Datumsüberschrift enthält
    /// </summary>
    /// <param name="line">Zu prüfende Zeile</param>
    /// <returns>Gibt an, ob es sich um die Datumszeile handelt</returns>
    private static bool IsDateHeaderLine(string line)
    {
        return line.StartsWith(";;;;", StringComparison.Ordinal) &&
               line.Contains("31.03.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Parst die Stichtage aus der Datumsüberschrift
    /// </summary>
    /// <param name="line">Zeile mit Datumswerten</param>
    /// <returns>Liste der geparsten Stichtage</returns>
    private static List<DateTime> ParseReferenceDates(string line)
    {
        var result = new List<DateTime>();
        var parts = line.Split(';');

        foreach (var part in parts.Skip(4))
        {
            var value = part.Trim();

            if (DateTime.TryParseExact(
                    value,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                result.Add(parsedDate);
            }
        }

        return result;
    }

    /// <summary>
    /// Gibt an, ob eine Zeile eine Datenzeile ist
    /// </summary>
    /// <param name="line">Zu prüfende Zeile</param>
    /// <returns>Gibt an, ob die Zeile eine Datenzeile ist</returns>
    private static bool IsDataLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        return line.StartsWith("WZ08-", StringComparison.OrdinalIgnoreCase) ||
               line.StartsWith("OA;", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parst einen Beschäftigungswert
    /// </summary>
    /// <param name="rawValue">Rohwert aus der CSV-Zeile</param>
    /// <param name="employmentCount">Geparste Beschäftigtenzahl</param>
    /// <returns>Gibt an, ob der Wert erfolgreich geparst werden konnte</returns>
    private static bool TryParseEmploymentValue(string rawValue, out int employmentCount)
    {
        employmentCount = 0;

        if (string.IsNullOrWhiteSpace(rawValue) ||
            rawValue == "-")
        {
            return false;
        }

        return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out employmentCount);
    }
}