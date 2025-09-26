# Paquetes NuGet para Serilog - Gu�a Completa

## Paquetes Instalados

### 1. Paquetes Principales de Serilog
```xml
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
```

### 2. Comandos de Instalaci�n
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File  
dotnet add package Serilog.Settings.Configuration
```

## Descripci�n de Paquetes

### Serilog.AspNetCore
- **Prop�sito**: Integraci�n principal de Serilog con ASP.NET Core
- **Incluye**: 
  - `UseSerilog()` para configuraci�n del host
  - `UseSerilogRequestLogging()` para logging de requests HTTP
  - Integraci�n con Microsoft.Extensions.Logging

### Serilog.Sinks.Console
- **Prop�sito**: Salida de logs a la consola
- **Caracter�sticas**:
  - Colores en la consola (themes)
  - Formateo personalizable
  - Templates de salida configurables

### Serilog.Sinks.File
- **Prop�sito**: Salida de logs a archivos
- **Caracter�sticas**:
  - Rolling files (por d�a, tama�o, etc.)
  - Retenci�n de archivos (eliminar archivos antiguos)
  - Buffering para performance
  - Archivos separados por nivel (ej: errors.txt)

### Serilog.Settings.Configuration
- **Prop�sito**: Configuraci�n desde appsettings.json
- **Permite**:
  - Configurar sinks desde JSON
  - Cambiar niveles sin recompilar
  - Configuraci�n por ambiente

## Configuraci�n Implementada

### appsettings.json
```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "AzureApi": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "buffered": true
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/errors-.txt",
          "restrictedToMinimumLevel": "Error",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  }
}
```

### Program.cs
```csharp
// Bootstrap logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

// Configuraci�n completa
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/errors-.txt", restrictedToMinimumLevel: LogEventLevel.Error)
);

// Request logging
app.UseSerilogRequestLogging();
```

## Caracter�sticas Implementadas

### ? Logging a Consola
- Formateo con colores
- Template personalizado
- Filtrado por nivel

### ? Logging a Archivos
- Archivo general: `Logs/log-YYYYMMDD.txt`
- Archivo de errores: `Logs/errors-YYYYMMDD.txt`
- Rolling diario
- Retenci�n configurable

### ? Logging Estructurado
- Par�metros con nombre `{UserId}`
- Objetos complejos `{@User}`
- Contexto y scopes

### ? Configuraci�n Flexible
- Por ambiente (Development/Production)
- Desde appsettings.json
- Override por namespace

### ? Integraci�n ASP.NET Core
- Request logging autom�tico
- Inyecci�n de dependencias
- Performance logging

## Uso en el C�digo

### Inyecci�n de Dependencias
```csharp
public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;
    
    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }
}
```

### Logging Estructurado
```csharp
_logger.LogInformation("Usuario {UserId} ejecut� {Action}", userId, "login");
_logger.LogInformation("Datos: {@User}", user);
```

### Scopes y Contexto
```csharp
using var scope = _logger.BeginScope("OperationId_{OperationId}", Guid.NewGuid());
_logger.LogInformation("Operaci�n iniciada");
```

## Endpoints de Demostraci�n

- `GET /api/loggingdemo/levels` - Niveles de logging
- `POST /api/loggingdemo/structured` - Logging estructurado
- `GET /api/loggingdemo/context/{userId}` - Contexto y scopes
- `GET /api/loggingdemo/error` - Manejo de errores
- `GET /api/loggingdemo/performance` - M�tricas de performance

## Archivos de Log Generados

- `Logs/log-YYYYMMDD.txt` - Todos los logs
- `Logs/errors-YYYYMMDD.txt` - Solo errores y cr�ticos
- `Logs/dev-log-YYYYMMDD.txt` - Logs de desarrollo (m�s detallados)

## Paquetes Opcionales Adicionales

```bash
# Para logging a Azure Application Insights
dotnet add package Serilog.Sinks.ApplicationInsights

# Para logging a Elasticsearch
dotnet add package Serilog.Sinks.Elasticsearch

# Para logging con formateo JSON
dotnet add package Serilog.Formatting.Compact

# Para enriquecimiento adicional
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Process
```