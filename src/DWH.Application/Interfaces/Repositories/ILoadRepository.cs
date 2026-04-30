using DWH.Domain.Entities;

namespace DWH.Application.Interfaces.Repositories;

public interface ILoadRepository
{
    Task ImportAllFromCsvAsync(
        CsvImportFiles csvImportFiles,
        string importBaseUrl,
        CancellationToken cancellationToken = default);

    Task ImportAiExposuresFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default);

    Task ImportGermanEmploymentsFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default);

    Task ImportSalariesFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default);

    Task ImportIlostatInformationEmploymentsFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default);

    Task ImportEurostatJobVacanciesFromCsvAsync(
        string importBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default);
}

public class CsvImportFiles
{
    public required string AiExposuresFileName { get; set; }
    public required string GermanEmploymentsFileName { get; set; }
    public required string SalariesFileName { get; set; }
    public required string IlostatInformationEmploymentsFileName { get; set; }
    public required string EurostatJobVacanciesFileName { get; set; }
}