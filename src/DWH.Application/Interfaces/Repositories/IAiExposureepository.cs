using DWH.Domain.Dtos.AiExposure;

namespace DWH.Application.Interfaces.Repositories;

public interface IAiExposureRepository
{
    Task<AiExposureApiResponse<AiExposureOccupationDto>> GetOccupationsAsync(
        CancellationToken cancellationToken = default);
}