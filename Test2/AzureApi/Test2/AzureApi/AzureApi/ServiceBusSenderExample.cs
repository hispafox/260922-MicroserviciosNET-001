using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace AzureApi
{
    public class ServiceBusSenderExample
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public ServiceBusSenderExample(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task SendMessagesAsync()
        {
            await using var client = new ServiceBusClient(_connectionString);
            ServiceBusSender sender = client.CreateSender(_queueName);

            for (int i = 1; i <= 5; i++)
            {
                ServiceBusMessage message = new ServiceBusMessage($"Mensaje {i}");
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Enviado: Mensaje {i}");
            }
        }
    }
}
