using DWH.Application.Interfaces.Repositories;
using DWH.Application.Interfaces.Services;
using DWH.Application.Parsers;
using DWH.Domain.Dtos.Destatis;
using DWH.Domain.Entities;

namespace DWH.Application.Services;

public class ImportService(
    IDbRepository dbRepository,
    IDestatisRepository destatisRepository,
    IIlostatRepository ilostatRepository,
    IAiExposureRepository aiExposureRepository)
    : IImportService
{
    public async Task StartImportAsync(CancellationToken cancellationToken = default)
    {
        ImportState.Progress = 0;
        ImportState.IsRunning = true;
        ImportState.Message = "Import wird gestartet";

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            ImportState.Message = "Beschäftigungsdaten von Ilostat werden importiert";

            var ilostatInformationEmploymentItems =
                await ilostatRepository.GetInformationAndComunicationEmploymentStatistic(
                    cancellationToken: cancellationToken);

            //await dbRepository.ReplaceIlostatInformationEmploymentsAsync(ilostatInformationEmploymentItems, cancellationToken);
            
            ImportState.Progress = 25;

            ImportState.Message = "Beschäftigungsdaten von Destatis werden importiert";

            var employmentItems = await ImportDestatisEmployment(cancellationToken).ConfigureAwait(false);

            ImportState.Progress = 50;
            ImportState.Message = "KI-Expositionsdaten werden importiert";

            var aiExposureItems = await ImportAiExposureData(cancellationToken).ConfigureAwait(false);

            ImportState.Progress = 75;
            ImportState.Message = "Destatis-Gehälter-Daten werden importiert";

            var destatisItems = await ImportDestatisSalaryData(cancellationToken).ConfigureAwait(false);

            await dbRepository.ReplaceAllAsync(
                aiExposureItems,
                employmentItems,
                destatisItems,
                ilostatInformationEmploymentItems,
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
                AiExposureScore = o.RiskScore ?? 0,
                Occupation = o.Title ?? "Unknown"
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

        return DestatisParser.ParseSalaryDataResponse(rawJson, onlyItJobs: false);
    }
}