using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IO;

namespace AzureApi.Controllers
{
    /// <summary>
    /// Controlador para gestionar y verificar logs
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;
        private readonly string _logsPath;

        public LogsController(ILogger<LogsController> logger)
        {
            _logger = logger;
            _logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        }

        /// <summary>
        /// Genera logs de prueba para verificar que se escriben correctamente
        /// </summary>
        [HttpPost("test")]
        public IActionResult GenerateTestLogs()
        {
            _logger.LogTrace("Mensaje de TRACE - nivel más detallado");
            _logger.LogDebug("Mensaje de DEBUG - información de desarrollo");
            _logger.LogInformation("Mensaje de INFORMATION - información general importante");
            _logger.LogWarning("Mensaje de WARNING - advertencia sobre algo que podría ser problemático");
            _logger.LogError("Mensaje de ERROR - error simulado para pruebas");
            _logger.LogCritical("Mensaje de CRITICAL - error crítico simulado");

            // Logging estructurado
            _logger.LogInformation("Test con datos estructurados: Usuario {UserId} ejecutó {Action} a las {Timestamp}", 
                123, "GenerateTestLogs", DateTime.Now);

            // Logging con objetos complejos
            var testObject = new { Id = 1, Name = "Test User", Data = new { Value = 42, Status = "Active" } };
            _logger.LogInformation("Objeto de prueba: {@TestObject}", testObject);

            // Usando Serilog directamente
            Log.Information("Log directo de Serilog desde controlador");
            Log.Error("Error directo de Serilog desde controlador");

            return Ok(new
            {
                message = "Logs de prueba generados exitosamente",
                timestamp = DateTime.Now,
                logsDirectory = _logsPath,
                expectedFiles = new[]
                {
                    $"log-{DateTime.Now:yyyyMMdd}.txt",
                    $"errors-{DateTime.Now:yyyyMMdd}.txt"
                }
            });
        }

        /// <summary>
        /// Lista los archivos de log disponibles
        /// </summary>
        [HttpGet("files")]
        public IActionResult GetLogFiles()
        {
            try
            {
                if (!Directory.Exists(_logsPath))
                {
                    return Ok(new
                    {
                        message = "Directorio de logs no existe aún",
                        logsPath = _logsPath,
                        files = new string[0]
                    });
                }

                var logFiles = Directory.GetFiles(_logsPath, "*.txt")
                    .Select(f => new FileInfo(f))
                    .Select(fi => new
                    {
                        Name = fi.Name,
                        Size = fi.Length,
                        SizeFormatted = FormatFileSize(fi.Length),
                        LastModified = fi.LastWriteTime,
                        FullPath = fi.FullName
                    })
                    .OrderByDescending(f => f.LastModified)
                    .ToArray();

                return Ok(new
                {
                    logsPath = _logsPath,
                    totalFiles = logFiles.Length,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener archivos de log");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lee las últimas líneas de un archivo de log específico
        /// </summary>
        [HttpGet("files/{fileName}/tail")]
        public async Task<IActionResult> TailLogFile(string fileName, [FromQuery] int lines = 50)
        {
            try
            {
                var filePath = Path.Combine(_logsPath, fileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { error = $"Archivo {fileName} no encontrado" });
                }

                // Leer las últimas líneas del archivo
                var allLines = await System.IO.File.ReadAllLinesAsync(filePath);
                var tailLines = allLines.TakeLast(lines).ToArray();

                var fileInfo = new FileInfo(filePath);

                return Ok(new
                {
                    fileName,
                    filePath,
                    totalLines = allLines.Length,
                    requestedLines = lines,
                    returnedLines = tailLines.Length,
                    fileSize = fileInfo.Length,
                    fileSizeFormatted = FormatFileSize(fileInfo.Length),
                    lastModified = fileInfo.LastWriteTime,
                    content = tailLines
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer archivo de log {FileName}", fileName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica el estado del sistema de logging
        /// </summary>
        [HttpGet("health")]
        public IActionResult GetLoggingHealth()
        {
            var health = new
            {
                logsDirectory = _logsPath,
                directoryExists = Directory.Exists(_logsPath),
                canWriteToDirectory = CanWriteToDirectory(_logsPath),
                expectedTodayFiles = new[]
                {
                    $"log-{DateTime.Now:yyyyMMdd}.txt",
                    $"errors-{DateTime.Now:yyyyMMdd}.txt"
                },
                actualFiles = Directory.Exists(_logsPath) 
                    ? Directory.GetFiles(_logsPath, "*.txt").Select(Path.GetFileName).ToArray()
                    : new string[0],
                timestamp = DateTime.Now
            };

            // Generar un log de prueba
            _logger.LogInformation("Health check ejecutado desde {Endpoint}", "GET /api/logs/health");

            return Ok(health);
        }

        /// <summary>
        /// Limpia logs antiguos manualmente
        /// </summary>
        [HttpDelete("cleanup")]
        public IActionResult CleanupOldLogs([FromQuery] int keepDays = 7)
        {
            try
            {
                if (!Directory.Exists(_logsPath))
                {
                    return Ok(new { message = "No hay directorio de logs para limpiar" });
                }

                var cutoffDate = DateTime.Now.AddDays(-keepDays);
                var logFiles = Directory.GetFiles(_logsPath, "*.txt");
                var deletedFiles = new List<string>();

                foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        System.IO.File.Delete(file);
                        deletedFiles.Add(fileInfo.Name);
                        _logger.LogInformation("Archivo de log eliminado: {FileName}", fileInfo.Name);
                    }
                }

                return Ok(new
                {
                    message = $"Limpieza completada. {deletedFiles.Count} archivos eliminados",
                    keepDays,
                    cutoffDate,
                    deletedFiles,
                    remainingFiles = Directory.GetFiles(_logsPath, "*.txt").Select(Path.GetFileName).ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza de logs");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private bool CanWriteToDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var testFile = Path.Combine(path, $"write-test-{Guid.NewGuid()}.tmp");
                System.IO.File.WriteAllText(testFile, "test");
                System.IO.File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}