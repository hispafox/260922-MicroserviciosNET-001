using ApiServiceBusMassTransit.Models;
using MassTransit;
// Consumer implementation
public class SimpleMessageConsumer : IConsumer<SimpleMessage>
{
    private readonly ILogger<SimpleMessageConsumer> _logger;

    public SimpleMessageConsumer(ILogger<SimpleMessageConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SimpleMessage> context)
    {
        try
        {
            _logger.LogInformation("=== MENSAJE RECIBIDO (TOPICS - NAMESPACE CORREGIDO) ===");
            _logger.LogInformation("Mensaje recibido: {MessageText}", context.Message.Text);
            _logger.LogInformation("MessageId: {MessageId}", context.MessageId);
            _logger.LogInformation("ConversationId: {ConversationId}", context.ConversationId);
            _logger.LogInformation("SourceAddress: {SourceAddress}", context.SourceAddress);
            _logger.LogInformation("Timestamp: {Timestamp}", DateTime.UtcNow);
            _logger.LogInformation("Message Type: {MessageType}", context.Message.GetType().FullName);
            
            Console.WriteLine($"=== MENSAJE RECIBIDO (TOPICS - NAMESPACE CORREGIDO) ===");
            Console.WriteLine($"Mensaje recibido: {context.Message.Text}");
            Console.WriteLine($"MessageId: {context.MessageId}");
            Console.WriteLine($"SourceAddress: {context.SourceAddress}");
            Console.WriteLine($"Message Type: {context.Message.GetType().FullName}");
            Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"=======================================================");
            
            // Simular procesamiento exitoso
            await Task.Delay(100);
            
            _logger.LogInformation("Mensaje procesado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar mensaje: {MessageText}", context.Message.Text);
            throw; // Re-lanzar para que MassTransit maneje el retry
        }
    }
}
