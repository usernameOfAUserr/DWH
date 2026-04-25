namespace DWH.Application.Interfaces.Services;

public interface IStagingService
{
    Task RebuildWarehouseAsync(CancellationToken cancellationToken = default);
}