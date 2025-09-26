using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureApi
{
    public class ServiceBusSenderExample
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger? _logger;

        public ServiceBusSenderExample(string connectionString, string queueName, ILogger? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _logger = logger;
        }

        /// <summary>
        /// Env�a una cantidad espec�fica de mensajes de prueba
        /// </summary>
        /// <param name="messageCount">N�mero de mensajes a enviar</param>
        public async Task SendMessagesAsync(int messageCount = 5)
        {
            if (messageCount <= 0)
                throw new ArgumentException("El n�mero de mensajes debe ser mayor a cero", nameof(messageCount));

            try
            {
                await using var client = new ServiceBusClient(_connectionString);
                ServiceBusSender sender = client.CreateSender(_queueName);

                _logger?.LogInformation("Iniciando env�o de {MessageCount} mensajes a la cola {QueueName}", messageCount, _queueName);

                for (int i = 1; i <= messageCount; i++)
                {
                    var messageContent = $"Mensaje {i} de {messageCount} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    var message = new ServiceBusMessage(messageContent)
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Subject = "TestMessage",
                        ApplicationProperties =
                        {
                            ["MessageNumber"] = i,
                            ["TotalMessages"] = messageCount,
                            ["SentAt"] = DateTime.UtcNow
                        }
                    };

                    await sender.SendMessageAsync(message);
                    
                    _logger?.LogDebug("Mensaje enviado {Index}/{Total}: {Content}", i, messageCount, messageContent);
                    Console.WriteLine($"Enviado {i}/{messageCount}: {messageContent}");
                    
                    // Peque�a pausa entre mensajes para evitar saturar
                    if (i < messageCount)
                        await Task.Delay(100);
                }

                _logger?.LogInformation("Todos los mensajes enviados exitosamente. Total: {MessageCount}", messageCount);
                Console.WriteLine($"? Todos los {messageCount} mensajes enviados exitosamente");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error enviando mensajes a Service Bus");
                Console.WriteLine($"Error enviando mensajes: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// M�todo original para compatibilidad hacia atr�s
        /// </summary>
        public async Task SendMessagesAsync()
        {
            await SendMessagesAsync(5);
        }
    }
}
