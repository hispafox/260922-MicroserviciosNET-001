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
