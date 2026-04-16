using DWH.Domain.Entities;

namespace DWH.Application.Interfaces.Repositories;

public interface IDbRepository
{
    Task ReplaceAllAsync(
        IReadOnlyCollection<AiExposureData> aiExposures,
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        IReadOnlyCollection<DestatisSalaryData> salaries,
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        CancellationToken cancellationToken = default);

    Task ReplaceAiExposuresAsync(
        IReadOnlyCollection<AiExposureData> aiExposures,
        CancellationToken cancellationToken = default);

    Task ReplaceGermanEmploymentsAsync(
        IReadOnlyCollection<GermanEmploymentData> germanEmployments,
        CancellationToken cancellationToken = default);

    Task ReplaceSalariesAsync(
        IReadOnlyCollection<DestatisSalaryData> salaries,
        CancellationToken cancellationToken = default);

    Task ReplaceIlostatInformationEmploymentsAsync(
        IReadOnlyCollection<IlostatInformationEmploymentData> ilostatInformationEmployments,
        CancellationToken cancellationToken = default);
}