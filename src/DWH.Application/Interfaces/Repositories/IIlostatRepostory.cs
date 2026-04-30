using DWH.Domain.Dtos;
using DWH.Domain.Entities;
using DWH.Domain.Entities.Loading;

namespace DWH.Application.Interfaces.Repositories;

public interface IIlostatRepository
{
    Task<IReadOnlyCollection<IlostatInformationEmploymentData>> GetInformationAndComunicationEmploymentStatistic(
        CancellationToken cancellationToken = default);
}