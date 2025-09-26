using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace AzureApi
{
    public class ServiceBusReceiverExample
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public ServiceBusReceiverExample(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task ReceiveMessagesAsync()
        {
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusProcessor processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                string body = args.Message.Body.ToString();
                Console.WriteLine($"Recibido: {body}");
                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"Error: {args.Exception.Message}");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
            Console.WriteLine("Procesando mensajes. Presiona cualquier tecla para salir...");
            Console.ReadKey();
            await processor.StopProcessingAsync();
        }
    }
}
