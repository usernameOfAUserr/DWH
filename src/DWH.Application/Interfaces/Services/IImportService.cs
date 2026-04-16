namespace DWH.Application.Interfaces.Services;

public interface IImportService
{
    Task StartImportAsync(CancellationToken cancellationToken = default);
}