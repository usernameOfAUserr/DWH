using DWH.Application.Interfaces.Repositories;
using DWH.Application.Interfaces.Services;

namespace DWH.Application.Services;

public sealed class StagingService(IStagingRepository stagingRepository) : IStagingService
{
    public async Task RebuildWarehouseAsync(CancellationToken cancellationToken = default)
    {
        await TruncateWarehouseAsync(cancellationToken);

        await BuildDimensionsAsync(cancellationToken);
        await BuildFactsAsync(cancellationToken);
    }

    private async Task TruncateWarehouseAsync(CancellationToken cancellationToken)
    {
        await stagingRepository.TruncateFactAiExposureAsync(cancellationToken);
        await stagingRepository.TruncateFactSalaryAsync(cancellationToken);
        await stagingRepository.TruncateFactEmploymentIndustryAsync(cancellationToken);
        await stagingRepository.TruncateFactJobVacancyAsync(cancellationToken);
        await stagingRepository.TruncateDimJobAsync(cancellationToken);
        await stagingRepository.TruncateDimTimeAsync(cancellationToken);
        await stagingRepository.TruncateDimGeoAsync(cancellationToken);
    }

    private async Task BuildDimensionsAsync(CancellationToken cancellationToken)
    {
        await stagingRepository.BuildDimTimeAsync(cancellationToken);
        await stagingRepository.BuildDimJobAsync(cancellationToken);
        await stagingRepository.BuildDimGeoAsync(cancellationToken);
    }

    private async Task BuildFactsAsync(CancellationToken cancellationToken)
    {
        await stagingRepository.BuildFactJobVacancyAsync(cancellationToken);
        await stagingRepository.BuildFactAiExposureAsync(cancellationToken);
        await stagingRepository.BuildFactSalaryAsync(cancellationToken);
        await stagingRepository.BuildFactEmploymentIndustryAsync(cancellationToken);
    }
}