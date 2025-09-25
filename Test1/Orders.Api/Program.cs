using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orders.Api.Telemetry;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddTransient<Orders.Api.Handlers.CorrelationHandler>();

// Configura la BaseAddress del HttpClient usando el valor de appsettings.json
var menuApiBaseUrl = builder.Configuration["MenuApi:BaseUrl"];
builder.Services.AddHttpClient<Orders.Api.Clients.IMenuClient, Orders.Api.Clients.MenuClient>(client =>
{
    client.BaseAddress = new Uri(menuApiBaseUrl);
})
.AddHttpMessageHandler<Orders.Api.Handlers.CorrelationHandler>();

// OpenTelemetry configuration
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = httpContext =>
                    !httpContext.Request.Path.Value?.Contains("/health") ?? true;
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSource("MenuClient")
            .AddSource("OrderService")
            .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault()
                .AddService("Orders.Api", "1.0.0"))
            .AddJaegerExporter()
            .AddConsoleExporter()
            .AddProcessor(new SimpleActivityExportProcessor(new PlainTextFileExporter("otel-traces.txt")));
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
