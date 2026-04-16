using DWH.Application.Interfaces.Services;
using DWH.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DWH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController(IImportService importService, ILogger<ImportController> logger) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> StartImport(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Import-Start wurde angefordert");

        await importService.StartImportAsync(cancellationToken);

        logger.LogInformation("Import wurde erfolgreich gestartet");

        return Ok();
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        logger.LogInformation(
            "Import-Status wurde abgefragt. IsRunning: {IsRunning}, Progress: {Progress}, Message: {Message}",
            ImportState.IsRunning,
            ImportState.Progress,
            ImportState.Message);

        return Ok(new
        {
            ImportState.IsRunning,
            ImportState.Progress,
            ImportState.Message,
        });
    }
}