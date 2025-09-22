param(
  [string]$SolutionName = "GastroInMemory",
  [string]$Root = ".",
  # Connection strings (usar SQL Server local o ajustar a Docker/localdb)
  [string]$ConnIdentity = "Server=localhost;Database=identity;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$ConnMenu = "Server=localhost;Database=menu;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$ConnOrders = "Server=localhost;Database=orders;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$ConnPayments = "Server=localhost;Database=payments;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$ConnDelivery = "Server=localhost;Database=delivery;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$ConnNotifications = "Server=localhost;Database=notifications;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$ConnAnalytics = "Server=localhost;Database=analytics;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=true",
  [string]$AuthAuthority = "http://localhost:5000",
  [string]$SigningKey = "dev_signing_key_replace_in_prod"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 0) Preparar carpetas y solución
if ($Root -ne ".") { New-Item -ItemType Directory -Force -Path $Root   }
Set-Location $Root
New-Item -ItemType Directory -Force -Path "src","tools"  
dotnet new sln -n $SolutionName  

# 1) Proyectos base
$projects = @(
  # Host único
  @{ Name="Demo.Api"; Type="webapi"; Path="src\Demo.Api"; Args="--use-controllers" },
  # Contratos
  @{ Name="Integration.Contracts"; Type="classlib"; Path="src\Integration.Contracts"; Args="" },
  # Identity
  @{ Name="Identity.Domain"; Type="classlib"; Path="src\Identity\Identity.Domain"; Args="" },
  @{ Name="Identity.Application"; Type="classlib"; Path="src\Identity\Identity.Application"; Args="" },
  @{ Name="Identity.Infrastructure"; Type="classlib"; Path="src\Identity\Identity.Infrastructure"; Args="" },
  # Menu
  @{ Name="Menu.Domain"; Type="classlib"; Path="src\Menu\Menu.Domain"; Args="" },
  @{ Name="Menu.Application"; Type="classlib"; Path="src\Menu\Menu.Application"; Args="" },
  @{ Name="Menu.Infrastructure"; Type="classlib"; Path="src\Menu\Menu.Infrastructure"; Args="" },
  # Orders
  @{ Name="Orders.Domain"; Type="classlib"; Path="src\Orders\Orders.Domain"; Args="" },
  @{ Name="Orders.Application"; Type="classlib"; Path="src\Orders\Orders.Application"; Args="" },
  @{ Name="Orders.Infrastructure"; Type="classlib"; Path="src\Orders\Orders.Infrastructure"; Args="" },
  # Payments
  @{ Name="Payments.Domain"; Type="classlib"; Path="src\Payments\Payments.Domain"; Args="" },
  @{ Name="Payments.Application"; Type="classlib"; Path="src\Payments\Payments.Application"; Args="" },
  @{ Name="Payments.Infrastructure"; Type="classlib"; Path="src\Payments\Payments.Infrastructure"; Args="" },
  # Delivery
  @{ Name="Delivery.Domain"; Type="classlib"; Path="src\Delivery\Delivery.Domain"; Args="" },
  @{ Name="Delivery.Application"; Type="classlib"; Path="src\Delivery\Delivery.Application"; Args="" },
  @{ Name="Delivery.Infrastructure"; Type="classlib"; Path="src\Delivery\Delivery.Infrastructure"; Args="" },
  # Notifications
  @{ Name="Notifications.Domain"; Type="classlib"; Path="src\Notifications\Notifications.Domain"; Args="" },
  @{ Name="Notifications.Application"; Type="classlib"; Path="src\Notifications\Notifications.Application"; Args="" },
  @{ Name="Notifications.Infrastructure"; Type="classlib"; Path="src\Notifications\Notifications.Infrastructure"; Args="" },
  # Analytics
  @{ Name="Analytics.Domain"; Type="classlib"; Path="src\Analytics\Analytics.Domain"; Args="" },
  @{ Name="Analytics.Application"; Type="classlib"; Path="src\Analytics\Analytics.Application"; Args="" },
  @{ Name="Analytics.Infrastructure"; Type="classlib"; Path="src\Analytics\Analytics.Infrastructure"; Args="" }
)

foreach ($p in $projects) {
  New-Item -ItemType Directory -Force -Path $p.Path  
  dotnet new $($p.Type) -n $($p.Name) -o $p.Path $($p.Args)  
  dotnet sln add "$($p.Path)\$($p.Name).csproj"  
}

# 2) Referencias de proyectos (Clean)# Demo.Api referencia todas las Application/Domain/Infrastructure necesarias
$refsDemo = @(
  "src\Integration.Contracts\Integration.Contracts.csproj",
  "src\Identity\Identity.Domain\Identity.Domain.csproj",
  "src\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj",

  "src\Menu\Menu.Domain\Menu.Domain.csproj",
  "src\Menu\Menu.Application\Menu.Application.csproj",
  "src\Menu\Menu.Infrastructure\Menu.Infrastructure.csproj",

  "src\Orders\Orders.Domain\Orders.Domain.csproj",
  "src\Orders\Orders.Application\Orders.Application.csproj",
  "src\Orders\Orders.Infrastructure\Orders.Infrastructure.csproj",

  "src\Payments\Payments.Domain\Payments.Domain.csproj",
  "src\Payments\Payments.Application\Payments.Application.csproj",
  "src\Payments\Payments.Infrastructure\Payments.Infrastructure.csproj",

  "src\Delivery\Delivery.Domain\Delivery.Domain.csproj",
  "src\Delivery\Delivery.Application\Delivery.Application.csproj",
  "src\Delivery\Delivery.Infrastructure\Delivery.Infrastructure.csproj",

  "src\Notifications\Notifications.Domain\Notifications.Domain.csproj",
  "src\Notifications\Notifications.Application\Notifications.Application.csproj",
  "src\Notifications\Notifications.Infrastructure\Notifications.Infrastructure.csproj",

  "src\Analytics\Analytics.Domain\Analytics.Domain.csproj",
  "src\Analytics\Analytics.Application\Analytics.Application.csproj",
  "src\Analytics\Analytics.Infrastructure\Analytics.Infrastructure.csproj"
)
foreach ($r in $refsDemo) {
  dotnet add "src\Demo.Api\Demo.Api.csproj" reference $r  
}

# Application -> Domain en cada módulo
$modules = @("Identity","Menu","Orders","Payments","Delivery","Notifications","Analytics")
foreach ($m in $modules) {
  dotnet add "src\$m\$m.Application\$m.Application.csproj" reference "src\$m\$m.Domain\$m.Domain.csproj"  
}

# 3) Paquetes NuGet# Demo.Api: MassTransit InMemory, Versioning, Swagger, JWT, OTEL
dotnet add "src\Demo.Api\Demo.Api.csproj" package MassTransit  
dotnet add "src\Demo.Api\Demo.Api.csproj" package Asp.Versioning.Mvc  
dotnet add "src\Demo.Api\Demo.Api.csproj" package Swashbuckle.AspNetCore  
dotnet add "src\Demo.Api\Demo.Api.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer  
dotnet add "src\Demo.Api\Demo.Api.csproj" package OpenTelemetry.Extensions.Hosting  
dotnet add "src\Demo.Api\Demo.Api.csproj" package OpenTelemetry.Instrumentation.AspNetCore  
dotnet add "src\Demo.Api\Demo.Api.csproj" package OpenTelemetry.Instrumentation.Http  
dotnet add "src\Demo.Api\Demo.Api.csproj" package OpenTelemetry.Instrumentation.SqlClient  

# Identity.Infrastructure
dotnet add "src\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj" package Microsoft.AspNetCore.Identity.EntityFrameworkCore  
dotnet add "src\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj" package Microsoft.AspNetCore.Identity.UI  
dotnet add "src\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.SqlServer  
dotnet add "src\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design  

# Infraestructura de todos los módulos: EF Core SqlServer + Design
foreach ($m in @("Menu","Orders","Payments","Delivery","Notifications","Analytics")) {
  dotnet add "src\$m\$m.Infrastructure\$m.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.SqlServer  
  dotnet add "src\$m\$m.Infrastructure\$m.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design  
}

# 4) Código: Integration.Contracts
@'
namespace Integration.Contracts;

public record PedidoCreado(Guid OrderId, decimal Total);
public record PagoAprobado(Guid OrderId);
public record PagoRechazado(Guid OrderId);
public record PedidoListoParaReparto(Guid OrderId);
public record AnaliticaActualizada(Guid OrderId, decimal Total, string PaymentStatus);
'@ | Out-File -Encoding UTF8 "src\Integration.Contracts\Events.cs"

# 5) Código: Identity# Domain Abstraction
@'
namespace Identity.Domain.Abstractions;
public interface ITokenService
{
    Task<string> CreateTokenAsync(string userId, string userName, IEnumerable<string> roles, IEnumerable<string> scopes, string? audience = null, CancellationToken ct = default);
}
'@ | Out-File -Encoding UTF8 "src\Identity\Identity.Domain\Abstractions.cs"
# Infrastructure DbContext + Token
@'
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;
public class AppIdentityDb : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppIdentityDb(DbContextOptions<AppIdentityDb> options) : base(options) { }
}
'@ | Out-File -Encoding UTF8 "src\Identity\Identity.Infrastructure\Persistence.cs"

@'
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Security;
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public Task<string> CreateTokenAsync(string userId, string userName, IEnumerable<string> roles, IEnumerable<string> scopes, string? audience = null, CancellationToken ct = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Auth:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new("scope", string.Join(' ', scopes))
        };
        if (!string.IsNullOrWhiteSpace(audience)) claims.Add(new Claim("aud", audience!));
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        var token = new JwtSecurityToken(
            issuer: _cfg["Auth:Authority"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);
        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
'@ | Out-File -Encoding UTF8 "src\Identity\Identity.Infrastructure\Security.cs"

# 6) Código: Menu (Domain + Infra + App)
@'
namespace Menu.Domain.Entities;
public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
'@ | Out-File -Encoding UTF8 "src\Menu\Menu.Domain\Entities.cs"

@'
using System.Linq;

namespace Menu.Domain.Abstractions;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    IQueryable<T> Query();
}
public interface IUnitOfWork
{
    IRepository<Menu.Domain.Entities.MenuItem> MenuItems { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
'@ | Out-File -Encoding UTF8 "src\Menu\Menu.Domain\Abstractions.cs"

@'
using Microsoft.EntityFrameworkCore;
using Menu.Domain.Entities;
using Menu.Domain.Abstractions;

namespace Menu.Infrastructure.Persistence;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }
    public DbSet<MenuItem> Items => Set<MenuItem>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MenuItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.Name);
        });
    }
}

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly MenuDbContext _db;
    public EfRepository(MenuDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public void Remove(T e) => _db.Set<T>().Remove(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly MenuDbContext _db;
    private IRepository<MenuItem>? _menu;
    public UnitOfWork(MenuDbContext db) => _db = db;
    public IRepository<MenuItem> MenuItems => _menu ??= new EfRepository<MenuItem>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
'@ | Out-File -Encoding UTF8 "src\Menu\Menu.Infrastructure\Persistence.cs"

@'
using Menu.Domain.Abstractions;
using Menu.Domain.Entities;

namespace Menu.Application;
public record MenuItemDto(int Id, string Name, decimal Price, int Stock);
public record CreateMenuItem(string Name, decimal Price, int Stock);
public record UpdateMenuItem(string Name, decimal Price, int Stock);
public class MenuService
{
    private readonly IUnitOfWork _uow;
    public MenuService(IUnitOfWork uow) => _uow = uow;
    public async Task<int> CreateAsync(CreateMenuItem cmd, CancellationToken ct)
    {
        if (cmd.Price <= 0) throw new ArgumentException("Precio inválido");
        var e = new MenuItem { Name = cmd.Name, Price = cmd.Price, Stock = cmd.Stock };
        await _uow.MenuItems.AddAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        return e.Id;
    }
    public async Task UpdateAsync(int id, UpdateMenuItem cmd, CancellationToken ct)
    {
        var e = await _uow.MenuItems.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
        if (cmd.Price <= 0) throw new ArgumentException("Precio inválido");
        e.Name = cmd.Name; e.Price = cmd.Price; e.Stock = cmd.Stock; e.UpdatedAt = DateTimeOffset.UtcNow;
        _uow.MenuItems.Update(e); await _uow.SaveChangesAsync(ct);
    }
}
'@ | Out-File -Encoding UTF8 "src\Menu\Menu.Application\Services.cs"

# 7) Código: Orders (Domain + Infra + App)
@'
namespace Orders.Domain.Entities;
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
    public decimal Total { get; set; }
    public string Status { get; set; } = "CREATED";
}
public class OrderLine
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
'@ | Out-File -Encoding UTF8 "src\Orders\Orders.Domain\Entities.cs"

@'
using System.Linq;

namespace Orders.Domain.Abstractions;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    IQueryable<T> Query();
}
public interface IUnitOfWork
{
    IRepository<Orders.Domain.Entities.Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
'@ | Out-File -Encoding UTF8 "src\Orders\Orders.Domain\Abstractions.cs"

@'
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Abstractions;

namespace Orders.Infrastructure.Persistence;
public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> Lines => Set<OrderLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>(e => { e.HasKey(x => x.Id); e.Property(x => x.Total).HasColumnType("decimal(18,2)"); });
        b.Entity<OrderLine>(e => { e.HasKey(x => x.Id); e.HasOne<Order>().WithMany(x => x.Lines).HasForeignKey(x => x.OrderId); });
    }
}
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly OrdersDbContext _db;
    public EfRepository(OrdersDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _db;
    private IRepository<Order>? _orders;
    public UnitOfWork(OrdersDbContext db) => _db = db;
    public IRepository<Order> Orders => _orders ??= new EfRepository<Order>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
'@ | Out-File -Encoding UTF8 "src\Orders\Orders.Infrastructure\Persistence.cs"

@'
using Orders.Domain.Abstractions;
using Orders.Domain.Entities;

namespace Orders.Application;
public record OrderLineCreate(int ProductId, int Quantity, decimal Price);
public record CreateOrder(Guid CustomerId, List<OrderLineCreate> Lines);
public class OrdersService
{
    private readonly IUnitOfWork _uow;
    public OrdersService(IUnitOfWork uow) { _uow = uow; }
    public async Task<Guid> CreateAsync(CreateOrder cmd, CancellationToken ct)
    {
        var o = new Order { CustomerId = cmd.CustomerId };
        foreach (var l in cmd.Lines)
            o.Lines.Add(new OrderLine { ProductId = l.ProductId, Quantity = l.Quantity, Price = l.Price });
        o.Total = o.Lines.Sum(x => x.Price * x.Quantity);
        await _uow.Orders.AddAsync(o, ct); await _uow.SaveChangesAsync(ct);
        return o.Id;
    }
    public async Task<object?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var o = await _uow.Orders.GetByIdAsync(id, ct); if (o is null) return null;
        return new { o.Id, o.CustomerId, o.Total, o.Status, Lines = o.Lines.Select(l => new { l.ProductId, l.Quantity, l.Price }) };
    }
}
'@ | Out-File -Encoding UTF8 "src\Orders\Orders.Application\Services.cs"

# 8) Código: Payments (Domain + Infra + App)
@'
namespace Payments.Domain.Entities;
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
'@ | Out-File -Encoding UTF8 "src\Payments\Payments.Domain\Entities.cs"

@'
using System.Linq;

namespace Payments.Domain.Abstractions;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    IQueryable<T> Query();
}
public interface IUnitOfWork
{
    IRepository<Payments.Domain.Entities.Payment> Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
'@ | Out-File -Encoding UTF8 "src\Payments\Payments.Domain\Abstractions.cs"

@'
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Abstractions;

namespace Payments.Infrastructure.Persistence;
public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
    public DbSet<Payment> Payments => Set<Payment>();
    protected override void OnModelCreating(ModelBuilder b) => b.Entity<Payment>(e => e.HasKey(x => x.Id));
}
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly PaymentsDbContext _db;
    public EfRepository(PaymentsDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentsDbContext _db;
    private IRepository<Payment>? _payments;
    public UnitOfWork(PaymentsDbContext db) => _db = db;
    public IRepository<Payment> Payments => _payments ??= new EfRepository<Payment>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
'@ | Out-File -Encoding UTF8 "src\Payments\Payments.Infrastructure\Persistence.cs"

@'
namespace Payments.Application;
using Payments.Domain.Abstractions;
using Payments.Domain.Entities;

public class PaymentsService
{
    private readonly IUnitOfWork _uow;
    private readonly Random _rng = new();
    public PaymentsService(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> AuthorizeAsync(Guid orderId, decimal total, CancellationToken ct)
    {
        var approved = _rng.NextDouble() < 0.8;
        var p = new Payment { OrderId = orderId, Status = approved ? "APPROVED" : "DECLINED" };
        await _uow.Payments.AddAsync(p, ct); await _uow.SaveChangesAsync(ct);
        return approved;
    }
}
'@ | Out-File -Encoding UTF8 "src\Payments\Payments.Application\Services.cs"

# 9) Código: Delivery (Domain + Infra + App)
@'
namespace Delivery.Domain.Entities;
public class DeliveryOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
'@ | Out-File -Encoding UTF8 "src\Delivery\Delivery.Domain\Entities.cs"

@'
using Microsoft.EntityFrameworkCore;
using Delivery.Domain.Entities;

namespace Delivery.Infrastructure.Persistence;
public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }
    public DbSet<DeliveryOrder> Deliveries => Set<DeliveryOrder>();
    protected override void OnModelCreating(ModelBuilder b) => b.Entity<DeliveryOrder>(e => e.HasKey(x => x.Id));
}
public interface IDeliveryRepository
{
    Task<DeliveryOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(DeliveryOrder d, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
public class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _db;
    public DeliveryRepository(DeliveryDbContext db) => _db = db;
    public Task<DeliveryOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) => _db.Deliveries.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    public async Task AddAsync(DeliveryOrder d, CancellationToken ct = default) => await _db.Deliveries.AddAsync(d, ct);
    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
'@ | Out-File -Encoding UTF8 "src\Delivery\Delivery.Infrastructure\Persistence.cs"

@'
using Delivery.Infrastructure.Persistence;
using Delivery.Domain.Entities;

namespace Delivery.Application;
public class DeliveryService
{
    private readonly IDeliveryRepository _repo;
    public DeliveryService(IDeliveryRepository repo) => _repo = repo;
    public async Task AssignAsync(Guid orderId, CancellationToken ct)
    {
        var ex = await _repo.GetByOrderIdAsync(orderId, ct);
        if (ex is null) await _repo.AddAsync(new DeliveryOrder { OrderId = orderId, Status = "ASSIGNED" }, ct);
        else { ex.Status = "ASSIGNED"; }
        await _repo.SaveAsync(ct);
    }
}
'@ | Out-File -Encoding UTF8 "src\Delivery\Delivery.Application\Services.cs"

# 10) Código: Notifications (Domain + Infra + App)
@'
namespace Notifications.Domain.Entities;
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string Type { get; set; } = "INFO";
    public string Message { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
'@ | Out-File -Encoding UTF8 "src\Notifications\Notifications.Domain\Entities.cs"

@'
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence;
public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder b) => b.Entity<Notification>(e => e.HasKey(x => x.Id));
}
'@ | Out-File -Encoding UTF8 "src\Notifications\Notifications.Infrastructure\Persistence.cs"

@'
using Notifications.Infrastructure.Persistence;
using Notifications.Domain.Entities;

namespace Notifications.Application;
public class NotificationsService
{
    private readonly NotificationsDbContext _db;
    public NotificationsService(NotificationsDbContext db) => _db = db;
    public async Task AddAsync(Guid orderId, string type, string message, CancellationToken ct)
    {
        await _db.Notifications.AddAsync(new Notification { OrderId = orderId, Type = type, Message = message }, ct);
        await _db.SaveChangesAsync(ct);
    }
}
'@ | Out-File -Encoding UTF8 "src\Notifications\Notifications.Application\Services.cs"

# 11) Código: Analytics (Domain + Infra + App)
@'
namespace Analytics.Domain.Entities;
public class OrderMetrics
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Total { get; set; }
    public string PaymentStatus { get; set; } = "UNKNOWN";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
'@ | Out-File -Encoding UTF8 "src\Analytics\Analytics.Domain\Entities.cs"

@'
using Microsoft.EntityFrameworkCore;
using Analytics.Domain.Entities;

namespace Analytics.Infrastructure.Persistence;
public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }
    public DbSet<OrderMetrics> Metrics => Set<OrderMetrics>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<OrderMetrics>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.OrderId);
        });
    }
}
public interface IOrderMetricsRepository
{
    Task<OrderMetrics?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(OrderMetrics m, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
public class OrderMetricsRepository : IOrderMetricsRepository
{
    private readonly AnalyticsDbContext _db;
    public OrderMetricsRepository(AnalyticsDbContext db) => _db = db;
    public Task<OrderMetrics?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => _db.Metrics.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    public async Task AddAsync(OrderMetrics m, CancellationToken ct = default) => await _db.Metrics.AddAsync(m, ct);
    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
'@ | Out-File -Encoding UTF8 "src\Analytics\Analytics.Infrastructure\Persistence.cs"

@'
using Analytics.Infrastructure.Persistence;

namespace Analytics.Application;
public class AnalyticsService
{
    private readonly IOrderMetricsRepository _repo;
    public AnalyticsService(IOrderMetricsRepository repo) => _repo = repo;
    public async Task OnOrderCreated(Guid orderId, decimal total, CancellationToken ct)
    {
        var existing = await _repo.GetByOrderIdAsync(orderId, ct);
        if (existing is null)
        {
            await _repo.AddAsync(new Domain.Entities.OrderMetrics { OrderId = orderId, Total = total, PaymentStatus = "UNKNOWN" }, ct);
            await _repo.SaveAsync(ct);
        }
    }
    public async Task OnPaymentUpdated(Guid orderId, bool approved, CancellationToken ct)
    {
        var m = await _repo.GetByOrderIdAsync(orderId, ct);
        if (m is null) return;
        m.PaymentStatus = approved ? "APPROVED" : "DECLINED";
        await _repo.SaveAsync(ct);
    }
}
'@ | Out-File -Encoding UTF8 "src\Analytics\Analytics.Application\Services.cs"

# 12) Código: Demo.Api Controllers (Auth, Menu, Pedidos) y Consumers + Program# AuthController
@'
using Identity.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly ITokenService _tokens;
    public AuthController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, ITokenService tokens)
    { _users = users; _signIn = signIn; _tokens = tokens; }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUser cmd)
    {
        var u = new IdentityUser { UserName = cmd.UserName, Email = cmd.Email };
        var r = await _users.CreateAsync(u, cmd.Password);
        if (!r.Succeeded) return BadRequest(r.Errors);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUser cmd)
    {
        var u = await _users.FindByNameAsync(cmd.UserName);
        if (u is null) return Unauthorized();
        var ok = await _signIn.CheckPasswordSignInAsync(u, cmd.Password, false);
        if (!ok.Succeeded) return Unauthorized();
        var roles = await _users.GetRolesAsync(u);
        var token = await _tokens.CreateTokenAsync(u.Id, u.UserName!, roles, cmd.Scopes, cmd.Audience);
        return Ok(new { access_token = token });
    }
}
public record RegisterUser(string UserName, string Email, string Password);
public record LoginUser(string UserName, string Password, string? Audience, IEnumerable<string> Scopes);
'@ | Out-File -Encoding UTF8 "src\Demo.Api\Controllers\AuthController.cs"

# MenuController
@'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Menu.Domain.Abstractions;
using Menu.Application;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/menu")]
[ApiVersion("1.0")]
public class MenuController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly MenuService _svc;
    public MenuController(IUnitOfWork uow, MenuService svc) { _uow = uow; _svc = svc; }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> Get(int page = 1, int size = 20, CancellationToken ct = default)
    {
        var q = _uow.MenuItems.Query().OrderBy(x => x.Name).Skip((page - 1) * size).Take(size);
        var items = await q.Select(i => new MenuItemDto(i.Id, i.Name, i.Price, i.Stock)).ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuItemDto>> GetById(int id, CancellationToken ct)
    {
        var i = await _uow.MenuItems.GetByIdAsync(id, ct);
        return i is null ? NotFound() : Ok(new MenuItemDto(i.Id, i.Name, i.Price, i.Stock));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateMenuItem cmd, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, null);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateMenuItem cmd, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, cmd, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var e = await _uow.MenuItems.GetByIdAsync(id, ct);
        if (e is null) return NotFound();
        _uow.MenuItems.Remove(e); await _uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
'@ | Out-File -Encoding UTF8 "src\Demo.Api\Controllers\MenuController.cs"

# PedidosController
@'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using Orders.Application;
using Integration.Contracts;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/pedidos")]
[ApiVersion("1.0")]
public class PedidosController : ControllerBase
{
    private readonly OrdersService _svc;
    private readonly IPublishEndpoint _publish;
    public PedidosController(OrdersService svc, IPublishEndpoint publish) { _svc = svc; _publish = publish; }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateOrder cmd, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(cmd, ct);
        await _publish.Publish(new PedidoCreado(id, cmd.Lines.Sum(x => x.Price * x.Quantity)), ct);
        return CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, null);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _svc.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
'@ | Out-File -Encoding UTF8 "src\Demo.Api\Controllers\PedidosController.cs"

# Consumers (PedidoCreado, PagoAprobado, PagoRechazado)
@'
using MassTransit;
using Integration.Contracts;
using Payments.Application;
using Analytics.Application;
using Delivery.Application;
using Notifications.Application;

public class PedidoCreadoConsumer : IConsumer<PedidoCreado>
{
    private readonly PaymentsService _payments;
    private readonly AnalyticsService _analytics;
    private readonly NotificationsService _notify;
    public PedidoCreadoConsumer(PaymentsService payments, AnalyticsService analytics, NotificationsService notify)
    { _payments = payments; _analytics = analytics; _notify = notify; }

    public async Task Consume(ConsumeContext<PedidoCreado> ctx)
    {
        await _analytics.OnOrderCreated(ctx.Message.OrderId, ctx.Message.Total, ctx.CancellationToken);
        await _notify.AddAsync(ctx.Message.OrderId, "INFO", $"Pedido creado por {ctx.Message.Total}", ctx.CancellationToken);
        var approved = await _payments.AuthorizeAsync(ctx.Message.OrderId, ctx.Message.Total, ctx.CancellationToken);
        if (approved) await ctx.Publish(new PagoAprobado(ctx.Message.OrderId));
        else await ctx.Publish(new PagoRechazado(ctx.Message.OrderId));
    }
}

public class PagoAprobadoConsumer : IConsumer<PagoAprobado>
{
    private readonly DeliveryService _delivery;
    private readonly AnalyticsService _analytics;
    private readonly NotificationsService _notify;
    public PagoAprobadoConsumer(DeliveryService delivery, AnalyticsService analytics, NotificationsService notify)
    { _delivery = delivery; _analytics = analytics; _notify = notify; }

    public async Task Consume(ConsumeContext<PagoAprobado> ctx)
    {
        await _delivery.AssignAsync(ctx.Message.OrderId, ctx.CancellationToken);
        await _analytics.OnPaymentUpdated(ctx.Message.OrderId, true, ctx.CancellationToken);
        await _notify.AddAsync(ctx.Message.OrderId, "SUCCESS", "Pago aprobado", ctx.CancellationToken);
        await ctx.Publish(new PedidoListoParaReparto(ctx.Message.OrderId));
    }
}

public class PagoRechazadoConsumer : IConsumer<PagoRechazado>
{
    private readonly AnalyticsService _analytics;
    private readonly NotificationsService _notify;
    public PagoRechazadoConsumer(AnalyticsService analytics, NotificationsService notify)
    { _analytics = analytics; _notify = notify; }

    public async Task Consume(ConsumeContext<PagoRechazado> ctx)
    {
        await _analytics.OnPaymentUpdated(ctx.Message.OrderId, false, ctx.CancellationToken);
        await _notify.AddAsync(ctx.Message.OrderId, "ERROR", "Pago rechazado", ctx.CancellationToken);
    }
}
'@ | Out-File -Encoding UTF8 "src\Demo.Api\Consumers.cs"

# Program.cs (Demo.Api)
@"
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
using Menu.Domain.Abstractions as MenuAbstractions;
using Menu.Application;

// Orders
using Orders.Infrastructure.Persistence;
using Orders.Domain.Abstractions as OrdersAbstractions;
using Orders.Application;

// Payments
using Payments.Infrastructure.Persistence;
using Payments.Domain.Abstractions as PaymentsAbstractions;
using Payments.Application;

// Delivery
using Delivery.Infrastructure.Persistence;
using Delivery.Application;
using Delivery.Infrastructure.Persistence as DeliveryInfra;

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
builder.Services.AddDbContext<AppIdentityDb>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Identity"")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDb>().AddDefaultTokenProviders();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddDbContext<MenuDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Menu"")));
builder.Services.AddDbContext<OrdersDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Orders"")));
builder.Services.AddDbContext<PaymentsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Payments"")));
builder.Services.AddDbContext<DeliveryDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Delivery"")));
builder.Services.AddDbContext<NotificationsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Notifications"")));
builder.Services.AddDbContext<AnalyticsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString(""Analytics"")));

// DI repos/UoW + servicios
builder.Services.AddScoped<MenuAbstractions.IUnitOfWork, Menu.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<MenuService>();

builder.Services.AddScoped<OrdersAbstractions.IUnitOfWork, Orders.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<OrdersService>();

builder.Services.AddScoped<PaymentsAbstractions.IUnitOfWork, Payments.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<PaymentsService>();

builder.Services.AddScoped<DeliveryInfra.IDeliveryRepository, DeliveryInfra.DeliveryRepository>();
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
builder.Services.AddAuthentication(""Bearer"").AddJwtBearer(o =>
{
    o.Authority = builder.Configuration[""Auth:Authority""];
    o.TokenValidationParameters.ValidateAudience = false;
});
builder.Services.AddAuthorization();

// Controllers + Versioning + Swagger
builder.Services.AddControllers();
builder.Services.AddApiVersioning(o => { o.AssumeDefaultVersionWhenUnspecified = true; o.DefaultApiVersion = new ApiVersion(1, 0); o.ReportApiVersions = true; })
    .AddApiExplorer(o => { o.GroupNameFormat = ""'v'VVV""; o.SubstituteApiVersionInUrl = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health (opcional)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppIdentityDb>()
    .AddDbContextCheck<MenuDbContext>()
    .AddDbContextCheck<OrdersDbContext>()
    .AddDbContextCheck<PaymentsDbContext>()
    .AddDbContextCheck<DeliveryDbContext>()
    .AddDbContextCheck<NotificationsDbContext>()
    .AddDbContextCheck<AnalyticsDbContext>();

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
app.UseSwagger(); app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks(""/health"");
app.Run();
"@ | Out-File -Encoding UTF8 "src\Demo.Api\Program.cs"

# 13) appsettings.json para Demo.Api
@"
{
  ""ConnectionStrings"": {
    ""Identity"": ""$ConnIdentity"",
    ""Menu"": ""$ConnMenu"",
    ""Orders"": ""$ConnOrders"",
    ""Payments"": ""$ConnPayments"",
    ""Delivery"": ""$ConnDelivery"",
    ""Notifications"": ""$ConnNotifications"",
    ""Analytics"": ""$ConnAnalytics""
  },
  ""Auth"": {
    ""Authority"": ""$AuthAuthority"",
    ""SigningKey"": ""$SigningKey""
  },
  ""Logging"": { ""LogLevel"": { ""Default"": ""Information"" } }
}
"@ | Out-File -Encoding UTF8 "src\Demo.Api\appsettings.json"

# 14) Restaurar y construir
dotnet restore  
dotnet build  

# 15) Instalar/actualizar EF CLI
dotnet tool update -g dotnet-ef  
$env:PATH += ";$([Environment]::GetFolderPath('UserProfile'))\.dotnet\tools"

# 16) Crear migraciones iniciales para cada contexto (startup = Demo.Api; project = Infrastructure)
$contexts = @(
  @{ Name="Identity"; Project="src\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj" },
  @{ Name="Menu"; Project="src\Menu\Menu.Infrastructure\Menu.Infrastructure.csproj" },
  @{ Name="Orders"; Project="src\Orders\Orders.Infrastructure\Orders.Infrastructure.csproj" },
  @{ Name="Payments"; Project="src\Payments\Payments.Infrastructure\Payments.Infrastructure.csproj" },
  @{ Name="Delivery"; Project="src\Delivery\Delivery.Infrastructure\Delivery.Infrastructure.csproj" },
  @{ Name="Notifications"; Project="src\Notifications\Notifications.Infrastructure\Notifications.Infrastructure.csproj" },
  @{ Name="Analytics"; Project="src\Analytics\Analytics.Infrastructure\Analytics.Infrastructure.csproj" }
)
foreach ($c in $contexts) {
  Write-Host "Creando migración Initial para $($c.Name)..."
  dotnet ef migrations add Initial -s "src\Demo.Api\Demo.Api.csproj" -p $c.Project  
}

Write-Host "Listo. Ejecuta: dotnet run --project src\Demo.Api\Demo.Api.csproj"