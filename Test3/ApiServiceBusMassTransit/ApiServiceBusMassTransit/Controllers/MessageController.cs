using ApiServiceBusMassTransit.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ApiServiceBusMassTransit.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessageController> _logger;

    public MessageController(IPublishEndpoint publishEndpoint, ILogger<MessageController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] string text)
    {
        var messageId = Guid.NewGuid();
        
        try
        {
            _logger.LogInformation("=== ENVIANDO MENSAJE (TOPICS PATTERN) ===");
            _logger.LogInformation("MessageId: {MessageId}", messageId);
            _logger.LogInformation("Texto: {Text}", text);
            _logger.LogInformation("Patrón: PUBLISH a TOPIC");
            _logger.LogInformation("Timestamp: {Timestamp}", DateTime.UtcNow);
            
            var message = new SimpleMessage { Text = text };
            await _publishEndpoint.Publish(message, context =>
            {
                context.MessageId = messageId;
                context.TimeToLive = TimeSpan.FromMinutes(5);
            });
            
            _logger.LogInformation("=== MENSAJE PUBLICADO EXITOSAMENTE (TOPICS) ===");
            _logger.LogInformation("MessageId: {MessageId} publicado en topic", messageId);
            
            // También usar Serilog estático para logs adicionales
            Log.Information("Mensaje publicado a TOPIC: {Text} con ID: {MessageId}", text, messageId);
            
            Console.WriteLine($"=== MENSAJE PUBLICADO (TOPICS) ===");
            Console.WriteLine($"Mensaje: {text}");
            Console.WriteLine($"MessageId: {messageId}");
            Console.WriteLine($"Patrón: PUBLISH -> TOPIC");
            Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"=====================================");
            
            return Ok(new { 
                sent = true, 
                text, 
                messageId,
                pattern = "Publish",
                destination = "Topic (SimpleMessage)",
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== ERROR AL PUBLICAR MENSAJE ===");
            _logger.LogError("MessageId: {MessageId}", messageId);
            _logger.LogError("Error: {ErrorMessage}", ex.Message);
            
            Log.Error(ex, "Error al publicar mensaje: {Text} con ID: {MessageId}", text, messageId);
            
            return StatusCode(500, new { 
                error = ex.Message, 
                messageId,
                timestamp = DateTime.UtcNow 
            });
        }
    }
}
