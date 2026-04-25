using DWH.Domain.Dtos;

namespace DWH.Application.Interfaces.Repositories;

public interface IEurostatRepository
{
    Task<EurostatResponse> GetInformationCommunicationJobVacanciesAsync(
        CancellationToken cancellationToken = default);
}