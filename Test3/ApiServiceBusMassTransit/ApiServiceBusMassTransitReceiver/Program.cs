using ApiServiceBusMassTransit.Models;
using MassTransit;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("MassTransit", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/receiver-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog como el proveedor de logging
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // MassTransit configuration - PATRÓN TOPICS CON NAMESPACE CORRECTO
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<SimpleMessageConsumer>();
        x.UsingAzureServiceBus((context, cfg) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("ServiceBus") ?? "<MISSING_CONNECTION_STRING>";
            cfg.Host(connectionString);
            
            // CONFIGURACIÓN TOPICS - Usar SubscriptionEndpoint
            cfg.SubscriptionEndpoint<SimpleMessage>("simple-message-subscription", e =>
            {
                e.ConfigureConsumer<SimpleMessageConsumer>(context);
                
                // Configuraciones adicionales para manejo de errores
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                e.UseInMemoryOutbox();
            });
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();

    // Log de inicialización
    Log.Information("=== APLICACIÓN RECEIVER INICIADA (NAMESPACE CORREGIDO) ===");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Configuración: TOPICS con SubscriptionEndpoint");
    Log.Information("Namespace SimpleMessage: {Namespace}", typeof(SimpleMessage).FullName);
    Log.Information("Esperando mensajes de Service Bus...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

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
