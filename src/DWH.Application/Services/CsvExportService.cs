using System.Globalization;
using System.Text;
using DWH.Domain.Entities;
using DWH.Domain.Entities.Loading;

namespace DWH.Application.Services;

/// <summary>
/// Service zum Erstellen von CSV-Dateien für den Exasol-Import
/// </summary>
public class CsvExportService
{
    /// <summary>
    /// Erstellt alle CSV-Dateien für die übergebenen Datensätze
    /// </summary>
    public async Task<CsvExportResult> ExportAllAsync(
        string outputDirectory,
        IReadOnlyCollection<AiExposureData> aiExposures,
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        IReadOnlyCollection<DestatisSalaryData> salaries,
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        IReadOnlyCollection<EurostatJobVacancyData> eurostatJobVacancies,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(aiExposures);
        ArgumentNullException.ThrowIfNull(germanEmployments);
        ArgumentNullException.ThrowIfNull(salaries);
        ArgumentNullException.ThrowIfNull(ilostatInformationEmployments);
        ArgumentNullException.ThrowIfNull(eurostatJobVacancies);

        Directory.CreateDirectory(outputDirectory);

        var aiExposuresFilePath = await ExportAiExposuresAsync(
            outputDirectory,
            aiExposures,
            cancellationToken);

        var germanEmploymentsFilePath = await ExportGermanEmploymentsAsync(
            outputDirectory,
            germanEmployments,
            cancellationToken);

        var salariesFilePath = await ExportSalariesAsync(
            outputDirectory,
            salaries,
            cancellationToken);

        var ilostatInformationEmploymentsFilePath = await ExportIlostatInformationEmploymentsAsync(
            outputDirectory,
            ilostatInformationEmployments,
            cancellationToken);

        var eurostatJobVacanciesFilePath = await ExportEurostatJobVacanciesAsync(
            outputDirectory,
            eurostatJobVacancies,
            cancellationToken);

        return new CsvExportResult
        {
            AiExposuresFilePath = aiExposuresFilePath,
            GermanEmploymentsFilePath = germanEmploymentsFilePath,
            SalariesFilePath = salariesFilePath,
            IlostatInformationEmploymentsFilePath = ilostatInformationEmploymentsFilePath,
            EurostatJobVacanciesFilePath = eurostatJobVacanciesFilePath,
        };
    }

    public async Task<string> ExportAiExposuresAsync(
        string outputDirectory,
        IReadOnlyCollection<AiExposureData> aiExposures,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(aiExposures);

        Directory.CreateDirectory(outputDirectory);

        var filePath = Path.Combine(outputDirectory, "ai_exposures.csv");

        await using var writer = CreateWriter(filePath);

        await writer.WriteLineAsync(
            "OCCUPATION,AI_EXPOSURE_SCORE,EMPLOYMENT,MEDIAN_WAGE,PROJECTED_GROWTH_PCT,TOP_RISK_FACTORS,RISK_SCORE,TOP_SAFE_TASKS");

        foreach (var item in aiExposures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = string.Join(",",
                EscapeCsv(item.Occupation),
                item.AiExposureScore.ToString(CultureInfo.InvariantCulture),
                item.Employment.ToString(CultureInfo.InvariantCulture),
                item.MedianWage.ToString(CultureInfo.InvariantCulture),
                item.ProjectedGrowthPct.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(item.TopRiskFactors),
                item.RiskScore.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(item.TopSafeTasks));

            await writer.WriteLineAsync(line);
        }

        return filePath;
    }

    public async Task<string> ExportGermanEmploymentsAsync(
        string outputDirectory,
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(germanEmployments);

        Directory.CreateDirectory(outputDirectory);

        var filePath = Path.Combine(outputDirectory, "german_employment.csv");

        await using var writer = CreateWriter(filePath);

        await writer.WriteLineAsync(
            "REFERENCE_DATE,DATA_YEAR,ECONOMIC_SECTION_CODE,ECONOMIC_SECTION,EMPLOYMENT_COUNT");

        foreach (var item in germanEmployments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = string.Join(",",
                item.ReferenceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                item.Year.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(item.EconomicSectionCode),
                EscapeCsv(item.EconomicSection),
                item.EmploymentCount.ToString(CultureInfo.InvariantCulture));

            await writer.WriteLineAsync(line);
        }

        return filePath;
    }

    public async Task<string> ExportSalariesAsync(
        string outputDirectory,
        IReadOnlyCollection<DestatisSalaryData> salaries,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(salaries);

        Directory.CreateDirectory(outputDirectory);

        var filePath = Path.Combine(outputDirectory, "salaries.csv");

        await using var writer = CreateWriter(filePath);

        await writer.WriteLineAsync("DATA_YEAR,SALARY,JOB");

        foreach (var item in salaries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = string.Join(",",
                item.Year.ToString(CultureInfo.InvariantCulture),
                item.Salary.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(item.Job));

            await writer.WriteLineAsync(line);
        }

        return filePath;
    }

    public async Task<string> ExportIlostatInformationEmploymentsAsync(
        string outputDirectory,
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(ilostatInformationEmployments);

        Directory.CreateDirectory(outputDirectory);

        var filePath = Path.Combine(outputDirectory, "ilostat_information_employment_data.csv");

        await using var writer = CreateWriter(filePath);

        await writer.WriteLineAsync("REF_AREA,AREA_LABEL,SOURCE_LABEL,TOTAL,TIME");

        foreach (var item in ilostatInformationEmployments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var totalValue = item.Total.HasValue
                ? item.Total.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;

            var line = string.Join(",",
                EscapeCsv(item.RefArea),
                EscapeCsv(item.AreaLabel),
                EscapeCsv(item.SourceLabel),
                totalValue,
                EscapeCsv(item.Time));

            await writer.WriteLineAsync(line);
        }

        return filePath;
    }

    public async Task<string> ExportEurostatJobVacanciesAsync(
        string outputDirectory,
        IReadOnlyCollection<EurostatJobVacancyData> eurostatJobVacancies,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(eurostatJobVacancies);

        Directory.CreateDirectory(outputDirectory);

        var filePath = Path.Combine(outputDirectory, "eurostat_job_vacancies.csv");

        await using var writer = CreateWriter(filePath);

        await writer.WriteLineAsync(
            "FREQ,S_ADJ,NACE_R2,SIZECLAS,INDIC_EM,GEO,TIME_PERIOD,OBSERVATION_VALUE");

        foreach (var item in eurostatJobVacancies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = string.Join(",",
                "Q",
                EscapeCsv(item.SeasonalAdjustment),
                EscapeCsv(item.IndustryCode),
                EscapeCsv(item.SizeClass),
                EscapeCsv(item.IndicatorCode),
                EscapeCsv(item.CountryCode),
                EscapeCsv(item.TimePeriod),
                item.ObservationValue.ToString(CultureInfo.InvariantCulture));

            await writer.WriteLineAsync(line);
        }

        return filePath;
    }

    private static StreamWriter CreateWriter(string filePath)
    {
        var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        return new StreamWriter(stream, new UTF8Encoding(false))
        {
            NewLine = "\n",
        };
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var requiresQuoting =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r');

        if (!requiresQuoting)
        {
            return value;
        }

        var escapedValue = value.Replace("\"", "\"\"");

        return $"\"{escapedValue}\"";
    }
}

/// <summary>
/// Ergebnis eines CSV-Exports
/// </summary>
public class CsvExportResult
{
    public required string AiExposuresFilePath { get; set; }

    public required string GermanEmploymentsFilePath { get; set; }

    public required string SalariesFilePath { get; set; }

    public required string IlostatInformationEmploymentsFilePath { get; set; }

    public required string EurostatJobVacanciesFilePath { get; set; }
}