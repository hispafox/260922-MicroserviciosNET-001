using Serilog;
using AzureApi.Services;

// Configurar Serilog temprano para capturar errores de inicio
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicación AzureApi...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog con la configuración completa
    builder.Host.UseSerilog((context, services, configuration) => 
    {
        // Crear directorio de logs si no existe
        var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logsDirectory);

        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "AzureApi")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logsDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.File(
                path: Path.Combine(logsDirectory, "errors-.txt"),
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);
    });

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddScoped<LoggingDemoService>();

    // Configure Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Azure Service Bus API",
            Version = "v1",
            Description = "API para demostrar el uso de Azure Service Bus con .NET 8 y Serilog"
        });
    });

    // Health checks
    builder.Services.AddHealthChecks();

    Log.Information("Servicios configurados correctamente");

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Service Bus API v1");
        // Comentar la línea que pone Swagger en la raíz para usar la ruta estándar /swagger
        // options.RoutePrefix = string.Empty; 
    });

    Log.Information("Swagger configurado en /swagger");

    // Agregar Serilog request logging
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();
    
    // Health check endpoint
    app.MapHealthChecks("/health");

    // Endpoint simple para verificar que funciona
    app.MapGet("/test", () => 
    {
        Log.Information("Endpoint /test llamado");
        return Results.Ok(new { 
            message = "API funcionando correctamente", 
            timestamp = DateTime.Now,
            environment = app.Environment.EnvironmentName
        });
    });

    Log.Information("Pipeline HTTP configurado correctamente");
    Log.Information("Swagger disponible en: https://localhost:7116/swagger o http://localhost:5xxx/swagger");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente: {Message}", ex.Message);
    return 1;
}
finally
{
    Log.Information("Cerrando aplicación...");
    await Log.CloseAndFlushAsync();
}

return 0;
