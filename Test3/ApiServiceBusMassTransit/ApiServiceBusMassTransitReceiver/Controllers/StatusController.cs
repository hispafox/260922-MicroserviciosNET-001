using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ApiServiceBusMassTransitReceiver.Controllers;

[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var status = new
        {
            status = "Receiver running",
            timestamp = DateTime.UtcNow,
            message = "Esperando mensajes de Service Bus...",
            logLocation = "logs/receiver-*.txt",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        _logger.LogInformation("Status consultado: {@Status}", status);
        Log.Information("Consulta de status realizada desde {RequestPath}", HttpContext.Request.Path);

        return Ok(status);
    }

    [HttpGet("logs")]
    public IActionResult GetLogsInfo()
    {
        try
        {
            var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var logFiles = Directory.Exists(logsPath)
                ? Directory.GetFiles(logsPath, "receiver-*.txt").Select(f => new
                {
                    fileName = Path.GetFileName(f),
                    size = new FileInfo(f).Length,
                    lastModified = new FileInfo(f).LastWriteTime
                }).ToArray()
                : Array.Empty<object>();

            var logsInfo = new
            {
                logsDirectory = logsPath,
                logFiles = logFiles,
                totalFiles = logFiles.Length,
                timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Información de logs consultada: {TotalFiles} archivos encontrados", logFiles.Length);

            return Ok(logsInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de logs");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}