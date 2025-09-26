using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AzureApi.Services
{
    /// <summary>
    /// Servicio de ejemplo que demuestra el uso de Serilog con logging estructurado
    /// </summary>
    public class LoggingDemoService
    {
        private readonly ILogger<LoggingDemoService> _logger;
        private static readonly Serilog.ILogger _serilogLogger = Log.ForContext<LoggingDemoService>();

        public LoggingDemoService(ILogger<LoggingDemoService> logger)
        {
            _logger = logger;
        }

        public void DemonstrateMicrosoftLogging()
        {
            // Usando Microsoft.Extensions.Logging (recomendado para inyecci�n de dependencias)
            _logger.LogInformation("Iniciando demostraci�n de logging");
            _logger.LogDebug("Este es un mensaje de debug con par�metro: {Parameter}", "valor");
            _logger.LogWarning("Advertencia con m�ltiples par�metros: {Param1} y {Param2}", "valor1", 42);
            
            try
            {
                throw new InvalidOperationException("Error de demostraci�n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturado durante la demostraci�n");
            }
        }

        public void DemonstrateSerilogDirectly()
        {
            // Usando Serilog directamente (�til para casos espec�ficos)
            _serilogLogger.Information("Log directo de Serilog");
            _serilogLogger.Debug("Debug con objeto estructurado: {@Object}", new { Id = 1, Name = "Test" });
            _serilogLogger.Warning("Advertencia con propiedades: {UserId} intent� {Action}", 123, "login");
            
            // Logging con contexto adicional usando Serilog directamente
            using (Serilog.Context.LogContext.PushProperty("OperationId", Guid.NewGuid()))
            {
                _serilogLogger.Information("Operaci�n dentro de contexto");
            }
        }

        public void DemonstrateStructuredLogging()
        {
            var user = new { Id = 1, Name = "Juan P�rez", Email = "juan@example.com" };
            var operation = new { Type = "ServiceBus", Action = "SendMessage", Queue = "test-queue" };

            // Logging estructurado - los objetos se serializan autom�ticamente
            _logger.LogInformation("Usuario {UserId} ejecut� operaci�n {@Operation} exitosamente", 
                user.Id, operation);

            // Logging con objetos complejos (usar @ para serializaci�n completa)
            _logger.LogInformation("Procesando usuario: {@User}", user);

            // M�tricas personalizadas
            _logger.LogInformation("Mensaje procesado en {Duration}ms con tama�o {Size} bytes", 
                150, 1024);
        }

        public async Task<string> ProcessWithLogging(string input)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["OperationId"] = Guid.NewGuid(),
                ["InputLength"] = input?.Length ?? 0
            });

            _logger.LogInformation("Iniciando procesamiento de entrada");

            try
            {
                await Task.Delay(100); // Simular trabajo
                var result = $"Procesado: {input?.ToUpper()}";
                
                _logger.LogInformation("Procesamiento completado exitosamente. Resultado: {ResultLength} caracteres", 
                    result.Length);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el procesamiento de entrada: {Input}", input);
                throw;
            }
        }
    }
}

/// <summary>
/// Extensiones �tiles para logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Extension method para logging de performance
    /// </summary>
    public static IDisposable BeginTimedOperation(this Microsoft.Extensions.Logging.ILogger logger, string operationName, params object[] args)
    {
        return new TimedOperation(logger, operationName, args);
    }

    private class TimedOperation : IDisposable
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly string _operationName;
        private readonly object[] _args;
        private readonly DateTime _startTime;

        public TimedOperation(Microsoft.Extensions.Logging.ILogger logger, string operationName, object[] args)
        {
            _logger = logger;
            _operationName = operationName;
            _args = args;
            _startTime = DateTime.UtcNow;
            
            _logger.LogDebug("Iniciando operaci�n: {OperationName}", _operationName);
        }

        public void Dispose()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            _logger.LogInformation("Operaci�n completada: {OperationName} en {ElapsedMs}ms", 
                _operationName, elapsed.TotalMilliseconds);
        }
    }
}