using DWH.Domain.Dtos;
using DWH.Domain.Entities;

namespace DWH.Application.Interfaces.Repositories;

public interface IIlostatRepository
{
    Task<IReadOnlyCollection<IlostatInformationEmploymentData>> GetInformationAndComunicationEmploymentStatistic(
        CancellationToken cancellationToken = default);
}