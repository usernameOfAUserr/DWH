using DWH.Domain.Dtos.Destatis;
using DWH.Domain.Models.DataSources;

namespace DWH.Application.Interfaces.Repositories;

public interface IDestatisRepository
{
    /// <summary>
    /// Liefert die technische Response der angebundenen Datenquelle
    /// </summary>
    /// <param name="request">Technischer Request für die Datenquelle</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Technische Response der Datenquelle</returns>
    Task<DestatisApiResponse> ListAsync(DestatisTableRequest request, CancellationToken cancellationToken = default);}