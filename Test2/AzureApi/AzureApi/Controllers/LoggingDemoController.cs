using Microsoft.AspNetCore.Mvc;
using AzureApi.Services;

namespace AzureApi.Controllers
{
    /// <summary>
    /// Controlador para demostrar las capacidades de logging con Serilog
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoggingDemoController : ControllerBase
    {
        private readonly ILogger<LoggingDemoController> _logger;
        private readonly LoggingDemoService _loggingService;

        public LoggingDemoController(ILogger<LoggingDemoController> logger, LoggingDemoService loggingService)
        {
            _logger = logger;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Demuestra diferentes niveles de logging
        /// </summary>
        [HttpGet("levels")]
        public IActionResult TestLogLevels()
        {
            _logger.LogTrace("Este es un mensaje TRACE - muy detallado");
            _logger.LogDebug("Este es un mensaje DEBUG - información de desarrollo");
            _logger.LogInformation("Este es un mensaje INFORMATION - información general");
            _logger.LogWarning("Este es un mensaje WARNING - advertencia");
            _logger.LogError("Este es un mensaje ERROR - error simulado");
            _logger.LogCritical("Este es un mensaje CRITICAL - error crítico simulado");

            return Ok(new { message = "Revisa los logs en consola y archivos", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Demuestra logging estructurado con objetos
        /// </summary>
        [HttpPost("structured")]
        public IActionResult TestStructuredLogging([FromBody] TestData data)
        {
            using var operation = _logger.BeginTimedOperation("TestStructuredLogging");

            _logger.LogInformation("Recibida petición con datos: {@TestData}", data);

            _loggingService.DemonstrateStructuredLogging();
            _loggingService.DemonstrateMicrosoftLogging();
            _loggingService.DemonstrateSerilogDirectly();

            _logger.LogInformation("Procesamiento completado para usuario {UserId}", data?.UserId);

            return Ok(new { 
                message = "Logging estructurado completado", 
                processedData = data,
                timestamp = DateTime.UtcNow 
            });
        }

        /// <summary>
        /// Demuestra logging con contexto y scopes
        /// </summary>
        [HttpGet("context/{userId}")]
        public async Task<IActionResult> TestContextLogging(int userId)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["CorrelationId"] = Guid.NewGuid(),
                ["Operation"] = "ContextTest"
            });

            _logger.LogInformation("Iniciando operación con contexto para usuario {UserId}", userId);

            try
            {
                var result = await _loggingService.ProcessWithLogging($"Data for user {userId}");
                
                _logger.LogInformation("Operación completada exitosamente");
                
                return Ok(new { 
                    result, 
                    userId, 
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en operación para usuario {UserId}", userId);
                return StatusCode(500, new { error = ex.Message, userId });
            }
        }

        /// <summary>
        /// Simula un error para probar logging de excepciones
        /// </summary>
        [HttpGet("error")]
        public IActionResult TestErrorLogging()
        {
            try
            {
                _logger.LogInformation("Intentando generar error de demostración");
                throw new InvalidOperationException("Este es un error de demostración para probar logging");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturado en endpoint de demostración");
                return StatusCode(500, new { 
                    error = "Error de demostración generado", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Demuestra logging de performance con métricas
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> TestPerformanceLogging()
        {
            using var operation = _logger.BeginTimedOperation("PerformanceTest");
            
            _logger.LogInformation("Iniciando prueba de performance");

            var tasks = new List<Task>();
            
            // Simular múltiples operaciones concurrentes
            for (int i = 0; i < 5; i++)
            {
                var taskId = i;
                tasks.Add(Task.Run(async () =>
                {
                    using var taskScope = _logger.BeginScope("TaskId_{TaskId}", taskId);
                    _logger.LogDebug("Iniciando tarea {TaskId}", taskId);
                    
                    await Task.Delay(Random.Shared.Next(100, 500));
                    
                    _logger.LogDebug("Tarea {TaskId} completada", taskId);
                }));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Todas las tareas completadas. Total: {TaskCount}", tasks.Count);

            return Ok(new { 
                message = "Prueba de performance completada",
                tasksExecuted = tasks.Count,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    /// <summary>
    /// Modelo para testing de datos estructurados
    /// </summary>
    public class TestData
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }
}