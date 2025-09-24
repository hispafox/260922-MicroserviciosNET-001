using Analytics.Application;
// Analytics
using Analytics.Infrastructure.Persistence;
using Asp.Versioning;
using Delivery.Application;
// Delivery
using Delivery.Infrastructure.Persistence;
using Identity.Domain.Abstractions;
// Identity
using Identity.Infrastructure;
using Identity.Infrastructure.Security;
using MassTransit;
using Menu.Application;
using Menu.Domain.Abstractions;
// Menu
using Menu.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notifications.Application;
// Notifications
using Notifications.Infrastructure.Persistence;
using Orders.Application;
using Orders.Domain.Abstractions;
// Orders
using Orders.Infrastructure.Persistence;
using Payments.Application;
using Payments.Domain.Abstractions;
// Payments
using Payments.Infrastructure.Persistence;
// Consumers
// (PedidoCreadoConsumer, PagoAprobadoConsumer, PagoRechazadoConsumer están en Consumers.cs)

using Serilog;
using System.Text; // <-- Añadido para Serilog

// Configura Serilog antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Reemplaza el logger por defecto por Serilog
builder.Host.UseSerilog();

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
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequiredLength = 12; // Más largo
    opts.Password.RequireUppercase = true;
    opts.Password.RequireLowercase = true; // Añadido
    opts.Password.RequireNonAlphanumeric = true; // Añadido
    opts.User.RequireUniqueEmail = true;

    // Opcional: bloqueo tras intentos fallidos
    opts.Lockout.MaxFailedAccessAttempts = 5;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    opts.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppIdentityDb>()
.AddDefaultTokenProviders();



builder.Services.AddScoped<ITokenService, JwtTokenService>();
// Agregar sección de configuración "Auth" para inyectar en JwtTokenService
//builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection("Auth"));

//builder.Services.Configure<JwtSettings>(
//    builder.Configuration.GetSection("JwtSettings"));


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


//// Auth demo (JWT)
//builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
//{
//    o.Authority = builder.Configuration["Auth:Authority"];
//    o.TokenValidationParameters.ValidateAudience = false;
//    o.RequireHttpsMetadata = false; // Permite HTTP en desarrollo

//});




//builder.Services.AddAuthorization();



// Configuración de JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsASecretKeyForJWTTokenAndItShouldBeLongEnough";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://yourdomain.com";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});




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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT en el campo: Bearer {token}"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

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

    // Seeding de menús
    var menuDb = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
    MenuSeeder.SeedAsync(menuDb).GetAwaiter().GetResult();
}

var configuration = app.Services.GetRequiredService<IConfiguration>();
bool crearDirecto = configuration.GetValue<bool>("Database:CreateDirecto");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MenuDbContext>();

    if (crearDirecto)
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        dbContext.Database.Migrate();
    }
}

// Proceso condicional para AppIdentityDb
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDb>();

    if (crearDirecto)
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        dbContext.Database.Migrate();
    }
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Asegúrate de cerrar y vaciar los logs al finalizar la app
Log.CloseAndFlush();










