namespace DWH.Application.Interfaces.Repositories;

public interface IStagingRepository
{
    Task TruncateFactAiExposureAsync(CancellationToken cancellationToken = default);

    Task TruncateFactSalaryAsync(CancellationToken cancellationToken = default);

    Task TruncateFactJobVacancyAsync(CancellationToken cancellationToken = default);

    Task TruncateFactEmploymentIndustryAsync(CancellationToken cancellationToken = default);

    Task TruncateDimTimeAsync(CancellationToken cancellationToken = default);


    Task BuildDimTimeAsync(CancellationToken cancellationToken = default);

    Task BuildFactJobVacancyAsync(CancellationToken cancellationToken = default);

    Task BuildDimJobAsync(CancellationToken cancellationToken = default);

    Task BuildFactAiExposureAsync(CancellationToken cancellationToken = default);

    Task BuildFactSalaryAsync(CancellationToken cancellationToken = default);

    Task BuildFactEmploymentIndustryAsync(CancellationToken cancellationToken = default);
}