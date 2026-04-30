using DWH.Application.Interfaces.Repositories;
using DWH.Application.Interfaces.Services;

namespace DWH.Application.Services;

public sealed class StagingService : IStagingService
{
    private readonly IStagingRepository _stagingRepository;

    public StagingService(IStagingRepository stagingRepository)
    {
        _stagingRepository = stagingRepository ?? throw new ArgumentNullException(nameof(stagingRepository));
    }

    public async Task RebuildWarehouseAsync(CancellationToken cancellationToken = default)
    {
        await TruncateWarehouseAsync(cancellationToken);

        await BuildDimensionsAsync(cancellationToken);
        await BuildFactsAsync(cancellationToken);
    }

    private async Task TruncateWarehouseAsync(CancellationToken cancellationToken)
    {
        await _stagingRepository.TruncateFactAiExposureAsync(cancellationToken);
        await _stagingRepository.TruncateFactSalaryAsync(cancellationToken);
        await _stagingRepository.TruncateFactEmploymentIndustryAsync(cancellationToken);
        await _stagingRepository.TruncateFactJobVacancyAsync(cancellationToken);
        await _stagingRepository.TruncateDimTimeAsync(cancellationToken);
    }

    private async Task BuildDimensionsAsync(CancellationToken cancellationToken)
    {
        await _stagingRepository.BuildDimTimeAsync(cancellationToken);
        await _stagingRepository.BuildDimJobAsync(cancellationToken);
    }

    private async Task BuildFactsAsync(CancellationToken cancellationToken)
    {
        await _stagingRepository.BuildFactJobVacancyAsync(cancellationToken);
        await _stagingRepository.BuildFactAiExposureAsync(cancellationToken);
        await _stagingRepository.BuildFactSalaryAsync(cancellationToken);
        await _stagingRepository.BuildFactEmploymentIndustryAsync(cancellationToken);
    }
}