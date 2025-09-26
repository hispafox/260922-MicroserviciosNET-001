using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureApi
{
    public class ServiceBusQueueInfo
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger? _logger;

        public ServiceBusQueueInfo(string connectionString, string queueName, ILogger? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _logger = logger;
        }

        /// <summary>
        /// Obtiene detalles de la cola de Service Bus
        /// </summary>
        /// <returns>Información detallada de la cola</returns>
        public async Task<object> GetQueueDetailsAsync()
        {
            try
            {
                var client = new ServiceBusAdministrationClient(_connectionString);
                
                // Verificar si la cola existe
                bool queueExists = await client.QueueExistsAsync(_queueName);
                
                if (!queueExists)
                {
                    return new
                    {
                        QueueName = _queueName,
                        Exists = false,
                        Message = "La cola no existe",
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Obtener propiedades de la cola
                var queueProperties = await client.GetQueueAsync(_queueName);
                var runtimeProperties = await client.GetQueueRuntimePropertiesAsync(_queueName);

                var queueInfo = new
                {
                    QueueName = _queueName,
                    Exists = true,
                    Properties = new
                    {
                        MaxSizeInMegabytes = queueProperties.Value.MaxSizeInMegabytes,
                        RequiresSession = queueProperties.Value.RequiresSession,
                        DeadLetteringOnMessageExpiration = queueProperties.Value.DeadLetteringOnMessageExpiration,
                        DefaultMessageTimeToLive = queueProperties.Value.DefaultMessageTimeToLive,
                        LockDuration = queueProperties.Value.LockDuration,
                        MaxDeliveryCount = queueProperties.Value.MaxDeliveryCount,
                        EnableBatchedOperations = queueProperties.Value.EnableBatchedOperations,
                        Status = queueProperties.Value.Status.ToString()
                    },
                    Runtime = new
                    {
                        ActiveMessageCount = runtimeProperties.Value.ActiveMessageCount,
                        DeadLetterMessageCount = runtimeProperties.Value.DeadLetterMessageCount,
                        ScheduledMessageCount = runtimeProperties.Value.ScheduledMessageCount,
                        TotalMessageCount = runtimeProperties.Value.TotalMessageCount,
                        SizeInBytes = runtimeProperties.Value.SizeInBytes,
                        CreatedAt = runtimeProperties.Value.CreatedAt,
                        UpdatedAt = runtimeProperties.Value.UpdatedAt,
                        AccessedAt = runtimeProperties.Value.AccessedAt
                    },
                    Timestamp = DateTime.UtcNow
                };

                _logger?.LogInformation("Información de cola obtenida exitosamente para {QueueName}", _queueName);
                return queueInfo;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error obteniendo información de la cola {QueueName}", _queueName);
                throw;
            }
        }
    }
}