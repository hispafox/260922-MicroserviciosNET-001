using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureApi
{
    public class ServiceBusReceiverExample
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger? _logger;

        public ServiceBusReceiverExample(string connectionString, string queueName, ILogger? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _logger = logger;
        }

        /// <summary>
        /// Recibe mensajes por un período de tiempo determinado
        /// </summary>
        /// <param name="duration">Duración para escuchar mensajes</param>
        /// <returns>Número de mensajes recibidos</returns>
        public async Task<int> ReceiveMessagesForDurationAsync(TimeSpan duration)
        {
            int messagesReceived = 0;

            try
            {
                await using var client = new ServiceBusClient(_connectionString);
                ServiceBusProcessor processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 1,
                    AutoCompleteMessages = false,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                      
                });

                processor.ProcessMessageAsync += async args =>
                {
                    try
                    {
                        string body = args.Message.Body.ToString();
                        string messageId = args.Message.MessageId;

                        _logger?.LogInformation("Mensaje recibido - ID: {MessageId}, Contenido: {Body}", messageId, body);
                        Console.WriteLine($"Recibido - ID: {messageId}, Contenido: {body}");

                        // Completar el mensaje para confirmarlo como procesado
                        await args.CompleteMessageAsync(args.Message);

                        Interlocked.Increment(ref messagesReceived);
                        _logger?.LogDebug("Mensaje completado exitosamente. Total procesados: {Count}", messagesReceived);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error procesando mensaje {MessageId}", args.Message.MessageId);
                        Console.WriteLine($"Error procesando mensaje: {ex.Message}");

                        // En caso de error, abandona el mensaje para reintentarlo
                        await args.AbandonMessageAsync(args.Message);
                    }
                };

                processor.ProcessErrorAsync += args =>
                {
                    _logger?.LogError(args.Exception, "Error en processor - Source: {Source}, Namespace: {Namespace}, Entity: {Entity}",
                        args.ErrorSource, args.FullyQualifiedNamespace, args.EntityPath);

                    Console.WriteLine($"Error en processor: {args.Exception.Message}");
                    Console.WriteLine($"Source: {args.ErrorSource}");
                    Console.WriteLine($"FullyQualifiedNamespace: {args.FullyQualifiedNamespace}");
                    Console.WriteLine($"EntityPath: {args.EntityPath}");

                    return Task.CompletedTask;
                };

                _logger?.LogInformation("Iniciando processor para recibir mensajes por {Duration} segundos", duration.TotalSeconds);
                Console.WriteLine($"Iniciando processor. Esperando mensajes por {duration.TotalSeconds} segundos...");

                await processor.StartProcessingAsync();

                // Esperar el tiempo especificado
                await Task.Delay(duration);

                _logger?.LogInformation("Tiempo de espera completado. Deteniendo processor...");
                Console.WriteLine("Tiempo de espera completado. Deteniendo processor...");

                await processor.StopProcessingAsync();

                _logger?.LogInformation("Processor detenido. Total de mensajes procesados: {Count}", messagesReceived);
                Console.WriteLine($"Processor detenido. Total mensajes procesados: {messagesReceived}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en ReceiveMessagesForDurationAsync");
                Console.WriteLine($"Error en recepción: {ex.Message}");
                throw;
            }

            return messagesReceived;
        }

        /// <summary>
        /// Método original para compatibilidad hacia atrás (ahora con timeout)
        /// </summary>
        public async Task ReceiveMessagesAsync()
        {
            await ReceiveMessagesForDurationAsync(TimeSpan.FromSeconds(30));
        }
    }
}
