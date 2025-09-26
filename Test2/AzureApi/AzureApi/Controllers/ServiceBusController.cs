using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace AzureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceBusController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger<ServiceBusController> _logger;

        public ServiceBusController(IConfiguration configuration, ILogger<ServiceBusController> logger)
        {
            _connectionString = configuration.GetConnectionString("AzureServiceBus") 
                              ?? configuration["AzureServiceBus:ConnectionString"] 
                              ?? throw new InvalidOperationException("Azure Service Bus connection string not found");
            _queueName = configuration["AzureServiceBus:QueueName"] 
                        ?? throw new InvalidOperationException("Azure Service Bus queue name not found");
            _logger = logger;
        }

        /// <summary>
        /// Env�a mensajes de prueba a la cola de Service Bus
        /// </summary>
        /// <param name="messageCount">N�mero de mensajes a enviar (default: 5, max: 100)</param>
        /// <returns>Resultado del env�o</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessages([FromQuery, Range(1, 100)] int messageCount = 5)
        {
            try
            {
                _logger.LogInformation("Iniciando env�o de {MessageCount} mensajes a la cola {QueueName}", 
                    messageCount, _queueName);

                var sender = new ServiceBusSenderExample(_connectionString, _queueName, _logger);
                await sender.SendMessagesAsync(messageCount);
                
                var result = new
                {
                    Success = true,
                    Message = $"{messageCount} mensajes enviados exitosamente",
                    QueueName = _queueName,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Mensajes enviados exitosamente: {@Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar mensajes a Service Bus");
                return StatusCode(500, new { 
                    Success = false,
                    Error = ex.Message, 
                    Timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Recibe mensajes de la cola de Service Bus por un tiempo determinado
        /// </summary>
        /// <param name="timeoutSeconds">Tiempo en segundos para escuchar mensajes (default: 30, max: 300)</param>
        /// <returns>Resultado de la recepci�n</returns>
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveMessages([FromQuery, Range(5, 300)] int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Iniciando recepci�n de mensajes por {TimeoutSeconds} segundos", timeoutSeconds);

                var receiver = new ServiceBusReceiverExample(_connectionString, _queueName, _logger);
                var receivedCount = await receiver.ReceiveMessagesForDurationAsync(TimeSpan.FromSeconds(timeoutSeconds));
                
                var result = new
                {
                    Success = true,
                    Message = $"Recepci�n completada. {receivedCount} mensajes procesados",
                    MessagesReceived = receivedCount,
                    TimeoutSeconds = timeoutSeconds,
                    QueueName = _queueName,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Recepci�n completada: {@Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recibir mensajes de Service Bus");
                return StatusCode(500, new { 
                    Success = false,
                    Error = ex.Message, 
                    Timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Obtiene informaci�n sobre la cola de Service Bus
        /// </summary>
        /// <returns>Informaci�n de la cola</returns>
        [HttpGet("queue-info")]
        public async Task<IActionResult> GetQueueInfo()
        {
            try
            {
                _logger.LogInformation("Obteniendo informaci�n de la cola {QueueName}", _queueName);

                var queueInfo = new ServiceBusQueueInfo(_connectionString, _queueName, _logger);
                var info = await queueInfo.GetQueueDetailsAsync();
                
                _logger.LogInformation("Informaci�n de cola obtenida: {@Info}", info);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener informaci�n de la cola");
                return StatusCode(500, new { 
                    Success = false,
                    Error = ex.Message, 
                    Timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar la configuraci�n
        /// </summary>
        /// <returns>Estado de la configuraci�n</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            var config = new
            {
                HasConnectionString = !string.IsNullOrEmpty(_connectionString),
                QueueName = _queueName,
                Timestamp = DateTime.UtcNow
            };

            return Ok(config);
        }
    }
}
