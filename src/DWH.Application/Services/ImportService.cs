using DWH.Application.Interfaces.Repositories;
using DWH.Application.Interfaces.Services;
using DWH.Application.Parsers;
using DWH.Domain.Dtos.Destatis;
using DWH.Domain.Entities;
using DWH.Domain.Entities.Loading;

namespace DWH.Application.Services;

public class ImportService(
    ILoadRepository loadRepository,
    IDestatisRepository destatisRepository,
    IIlostatRepository ilostatRepository,
    IAiExposureRepository aiExposureRepository,
    IEurostatRepository eurostatRepository,
    GithubFileUploadService githubFileUploadService)
    : IImportService
{
    private const string GithubOwner = "usernameOfAUserr";
    private const string GithubRepo = "DWH";
    private const string GithubBranch = "develop";
    private const string GithubTokenEnvironmentVariableName = "dwh_access_token_github";

    public async Task StartImportAsync(CancellationToken cancellationToken = default)
    {
        ImportState.Progress = 0;
        ImportState.IsRunning = true;
        ImportState.Message = "Import wird gestartet";

        try
        {
            
            ImportState.Message = "Eurostat-Stellenangebote werden importiert";

            var eurostatJobVacancyItems =
                await ImportEurostatJobVacancyData(cancellationToken).ConfigureAwait(false);
            
            var aiExposureItems = await ImportAiExposureData(cancellationToken).ConfigureAwait(false);

            ImportState.Message = "Beschäftigungsdaten von Ilostat werden importiert";

            var ilostatInformationEmploymentItems =
                await ilostatRepository.GetInformationAndComunicationEmploymentStatistic(
                    cancellationToken: cancellationToken);

            ImportState.Progress = 25;
            ImportState.Message = "Beschäftigungsdaten von Destatis werden importiert";

            var employmentItems = await ImportDestatisEmployment(cancellationToken).ConfigureAwait(false);

            ImportState.Progress = 50;
            ImportState.Message = "KI-Expositionsdaten werden importiert";

            ImportState.Progress = 75;
            ImportState.Message = "Destatis-Gehälter-Daten werden importiert";

            var destatisItems = await ImportDestatisSalaryData(cancellationToken).ConfigureAwait(false);

            ImportState.Message = "CSV-Dateien werden erzeugt";

            var csvExportService = new CsvExportService();
            var importDirectory = Path.Combine(AppContext.BaseDirectory, "import-files");

            var result = await csvExportService.ExportAllAsync(
                outputDirectory: importDirectory,
                aiExposures: aiExposureItems,
                germanEmployments: employmentItems,
                salaries: destatisItems,
                ilostatInformationEmployments: ilostatInformationEmploymentItems,
                eurostatJobVacancies: eurostatJobVacancyItems,
                cancellationToken: cancellationToken);

            ImportState.Progress = 85;
            ImportState.Message = "CSV-Dateien werden nach GitHub hochgeladen";

            var githubToken = Environment.GetEnvironmentVariable(GithubTokenEnvironmentVariableName);

            if (string.IsNullOrWhiteSpace(githubToken))
            {
                throw new InvalidOperationException(
                    $"Die Umgebungsvariable '{GithubTokenEnvironmentVariableName}' wurde nicht gefunden oder ist leer.");
            }

            var aiExposureUrl = await githubFileUploadService.UploadOrUpdateFileAsync(
                owner: GithubOwner,
                repo: GithubRepo,
                branch: GithubBranch,
                githubToken: githubToken,
                localFilePath: result.AiExposuresFilePath,
                targetPathInRepo: "imports/ai_exposures.csv",
                commitMessage: "Update ai_exposures.csv",
                cancellationToken: cancellationToken);

            var germainEmploymentUrl = await githubFileUploadService.UploadOrUpdateFileAsync(
                owner: GithubOwner,
                repo: GithubRepo,
                branch: GithubBranch,
                githubToken: githubToken,
                localFilePath: result.GermanEmploymentsFilePath,
                targetPathInRepo: "imports/german_employment.csv",
                commitMessage: "Update german_employment.csv",
                cancellationToken: cancellationToken);

            var salariesUrl = await githubFileUploadService.UploadOrUpdateFileAsync(
                owner: GithubOwner,
                repo: GithubRepo,
                branch: GithubBranch,
                githubToken: githubToken,
                localFilePath: result.SalariesFilePath,
                targetPathInRepo: "imports/salaries.csv",
                commitMessage: "Update salaries.csv",
                cancellationToken: cancellationToken);

            var ilostatisUrl = await githubFileUploadService.UploadOrUpdateFileAsync(
                owner: GithubOwner,
                repo: GithubRepo,
                branch: GithubBranch,
                githubToken: githubToken,
                localFilePath: result.IlostatInformationEmploymentsFilePath,
                targetPathInRepo: "imports/ilostat_information_employment_data.csv",
                commitMessage: "Update ilostat_information_employment_data.csv",
                cancellationToken: cancellationToken);

            var eurostatUrl = await githubFileUploadService.UploadOrUpdateFileAsync(
                owner: GithubOwner,
                repo: GithubRepo,
                branch: GithubBranch,
                githubToken: githubToken,
                localFilePath: result.EurostatJobVacanciesFilePath,
                targetPathInRepo: "imports/eurostat_job_vacancies.csv",
                commitMessage: "Update eurostat_job_vacancies.csv",
                cancellationToken: cancellationToken);

            ImportState.Progress = 90;
            ImportState.Message = "CSV-Dateien werden in Exasol importiert";

            var csvImportFiles = new CsvImportFiles
            {
                AiExposuresFileName = "ai_exposures.csv",
                GermanEmploymentsFileName = "german_employment.csv",
                SalariesFileName = "salaries.csv",
                IlostatInformationEmploymentsFileName = "ilostat_information_employment_data.csv",
                EurostatJobVacanciesFileName = "eurostat_job_vacancies.csv",
            };

            var importBaseUrl =
                $"https://raw.githubusercontent.com/{GithubOwner}/{GithubRepo}/{GithubBranch}/imports";

            await loadRepository.ImportAllFromCsvAsync(
                csvImportFiles,
                importBaseUrl,
                cancellationToken).ConfigureAwait(false);

            ImportState.Progress = 100;
            ImportState.Message = "Import abgeschlossen";
        }
        catch (Exception ex)
        {
            ImportState.Message = $"Import fehlgeschlagen: {ex.Message}";
            throw;
        }
        finally
        {
            ImportState.IsRunning = false;
        }
    }

    private async Task<IReadOnlyCollection<AiExposureData>> ImportAiExposureData(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await aiExposureRepository.GetOccupationsAsync(cancellationToken).ConfigureAwait(false);

        if (response.Data.Count == 0)
        {
            return [];
        }

        return response.Data
            .Select(o => new AiExposureData
            {
                Occupation = o.Title ?? "Unknown",

                AiExposureScore = o.GenAiExposure ?? 0,
                RiskScore = o.RiskScore ?? 0,

                Employment = o.Employment ?? 0,
                MedianWage = o.MedianWage ?? 0,
                ProjectedGrowthPct = o.ProjectedGrowthPct ?? 0,

                TopRiskFactors = o.TopRiskFactors != null
                    ? string.Join("; ", o.TopRiskFactors)
                    : string.Empty,

                TopSafeTasks = o.TopSafeTasks != null
                    ? string.Join("; ", o.TopSafeTasks)
                    : string.Empty
            })
            .ToList();
    }

    private async Task<IReadOnlyCollection<GermanEmploymentData>> ImportDestatisEmployment(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await destatisRepository.ListAsync(
            new DestatisTableRequest(
                Name: "13111-0004",
                StartYear: 2023,
                EndYear: 2025,
                Area: "all",
                Language: "de"),
            cancellationToken).ConfigureAwait(false);

        var rawJson = response.RawJson.GetRawText();

        return DestatisEmploymentParser.ParseEmploymentResponse(rawJson, false, false);
    }

    private async Task<IReadOnlyCollection<DestatisSalaryData>> ImportDestatisSalaryData(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await destatisRepository.ListAsync(
            new DestatisTableRequest(
                Name: "62361-0034",
                StartYear: 2020,
                EndYear: 2026,
                Area: "all",
                Language: "de"),
            cancellationToken).ConfigureAwait(false);

        var rawJson = response.RawJson.GetRawText();

        return DestatisParser.ParseSalaryDataResponse(rawJson, onlyItJobs: true);
    }

    private async Task<IReadOnlyCollection<EurostatJobVacancyData>> ImportEurostatJobVacancyData(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = await eurostatRepository
            .GetInformationCommunicationJobVacanciesAsync(cancellationToken)
            .ConfigureAwait(false);

        return EurostatJobVacancyParser.Parse(response.RawJson.RootElement.GetRawText());
    }
}