using Asp.Versioning;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Identity
using Identity.Infrastructure;
using Identity.Domain.Abstractions;
using Identity.Infrastructure.Security;

// Menu
using Menu.Infrastructure.Persistence;
using Menu.Domain.Abstractions;
using Menu.Application;

// Orders
using Orders.Infrastructure.Persistence;
using Orders.Domain.Abstractions;
using Orders.Application;

// Payments
using Payments.Infrastructure.Persistence;
using Payments.Domain.Abstractions;
using Payments.Application;

// Delivery
using Delivery.Infrastructure.Persistence;
using Delivery.Application;

// Notifications
using Notifications.Infrastructure.Persistence;
using Notifications.Application;

// Analytics
using Analytics.Infrastructure.Persistence;
using Analytics.Application;

// Consumers
// (PedidoCreadoConsumer, PagoAprobadoConsumer, PagoRechazadoConsumer están en Consumers.cs)

var builder = WebApplication.CreateBuilder(args);

// Configuración de cadenas (appsettings.json se crea más abajo)
bool isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddDbContext<AppIdentityDb>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Identity"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDb>().AddDefaultTokenProviders();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddDbContext<MenuDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Menu"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));
builder.Services.AddDbContext<OrdersDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Orders"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));
builder.Services.AddDbContext<PaymentsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Payments"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));
builder.Services.AddDbContext<DeliveryDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Delivery"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));
builder.Services.AddDbContext<NotificationsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Notifications"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));
builder.Services.AddDbContext<AnalyticsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Analytics"), sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(20),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(30);
})
.EnableServiceProviderCaching()
.EnableSensitiveDataLogging(isDevelopment));

// DI repos/UoW + servicios
builder.Services.AddScoped<Menu.Domain.Abstractions.IUnitOfWork, Menu.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<MenuService>();

builder.Services.AddScoped<Orders.Domain.Abstractions.IUnitOfWork, Orders.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<OrdersService>();

builder.Services.AddScoped<Payments.Domain.Abstractions.IUnitOfWork, Payments.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<PaymentsService>();

builder.Services.AddScoped<Delivery.Infrastructure.Persistence.IDeliveryRepository, Delivery.Infrastructure.Persistence.DeliveryRepository>();
builder.Services.AddScoped<DeliveryService>();

builder.Services.AddScoped<NotificationsService>();

builder.Services.AddScoped<Analytics.Infrastructure.Persistence.IOrderMetricsRepository, Analytics.Infrastructure.Persistence.OrderMetricsRepository>();
builder.Services.AddScoped<AnalyticsService>();

// MassTransit InMemory
builder.Services.AddMassTransit(cfg =>
{
    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.AddConsumer<PedidoCreadoConsumer>();
    cfg.AddConsumer<PagoAprobadoConsumer>();
    cfg.AddConsumer<PagoRechazadoConsumer>();
    cfg.UsingInMemory((context, bus) => { bus.ConfigureEndpoints(context); });
});

// Auth demo (JWT)
builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
{
    o.Authority = builder.Configuration["Auth:Authority"];
    o.TokenValidationParameters.ValidateAudience = false;
});
builder.Services.AddAuthorization();

// Controllers + Versioning + Swagger
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),     // /api/v1/products
        new QueryStringApiVersionReader(),    // ?version=1.0
        new HeaderApiVersionReader("X-Version") // Header: X-Version: 1.0
    );
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health (opcional)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Migraciones en arranque (solo demo)
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppIdentityDb>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<MenuDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>().Database.Migrate();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

