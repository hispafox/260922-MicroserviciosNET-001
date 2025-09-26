# Azure Service Bus API

Este proyecto demuestra cómo usar Azure Service Bus para enviar y recibir mensajes usando .NET 8 con Serilog para logging completo.

## Configuración

1. Actualiza `appsettings.json` o `appsettings.Development.json` con tu connection string de Azure Service Bus:

```json
{
  "AzureServiceBus": {
    "ConnectionString": "tu-connection-string-aqui",
    "QueueName": "tu-nombre-de-cola"
  }
}
```

## Endpoints Disponibles

### Azure Service Bus
- **POST** `/api/servicebus/send` - Envía mensajes de prueba a la cola
  - Query parameter: `messageCount` (1-100, default: 5)
- **POST** `/api/servicebus/receive` - Recibe mensajes de la cola por un tiempo determinado
  - Query parameter: `timeoutSeconds` (5-300, default: 30)
- **GET** `/api/servicebus/queue-info` - Obtiene información detallada sobre la cola
- **GET** `/api/servicebus/health` - Verifica el estado de la configuración

### Gestión de Logs
- **GET** `/api/logs/health` - Verifica el estado del sistema de logging
- **POST** `/api/logs/test` - Genera logs de prueba en todos los niveles
- **GET** `/api/logs/files` - Lista archivos de log disponibles con información de tamaño
- **GET** `/api/logs/files/{fileName}/tail` - Lee las últimas N líneas de un archivo de log
- **DELETE** `/api/logs/cleanup` - Limpia logs antiguos manualmente

### Demostración de Logging
- **GET** `/api/loggingdemo/levels` - Demuestra diferentes niveles de logging
- **POST** `/api/loggingdemo/structured` - Demuestra logging estructurado con objetos
- **GET** `/api/loggingdemo/context/{userId}` - Demuestra logging con contexto y scopes
- **GET** `/api/loggingdemo/error` - Simula errores para probar logging de excepciones
- **GET** `/api/loggingdemo/performance` - Demuestra logging de performance con métricas

## Sistema de Logging

### Archivos de Log Generados
- `Logs/log-YYYYMMDD.txt` - Todos los logs del día
- `Logs/errors-YYYYMMDD.txt` - Solo errores y críticos
- `Logs/dev-log-YYYYMMDD.txt` - Logs de desarrollo (más detallados)

### Características del Logging
- ? **Salida dual**: Consola y archivos simultáneamente
- ? **Rolling diario**: Nuevos archivos cada día
- ? **Retención automática**: 7 días logs generales, 30 días errores
- ? **Logging estructurado**: Parámetros con nombre y objetos serializados
- ? **Request logging**: Automático para todas las peticiones HTTP
- ? **Correlación**: IDs únicos para rastrear operaciones
- ? **Configuración flexible**: Por ambiente y namespace
- ? **Performance**: Buffering y flush automático

## Ejemplos de Uso

### Service Bus
```bash
# Enviar 10 mensajes
curl -X POST "https://localhost:7xxx/api/servicebus/send?messageCount=10"

# Recibir mensajes por 60 segundos
curl -X POST "https://localhost:7xxx/api/servicebus/receive?timeoutSeconds=60"

# Obtener información de la cola
curl -X GET "https://localhost:7xxx/api/servicebus/queue-info"
```

### Logging
```bash
# Generar logs de prueba
curl -X POST "https://localhost:7xxx/api/logs/test"

# Ver archivos de log
curl -X GET "https://localhost:7xxx/api/logs/files"

# Leer últimas 20 líneas del log principal
curl -X GET "https://localhost:7xxx/api/logs/files/log-20250109.txt/tail?lines=20"

# Verificar estado del logging
curl -X GET "https://localhost:7xxx/api/logs/health"
```

### Demostración de Logging Estructurado
```bash
# Probar diferentes niveles
curl -X GET "https://localhost:7xxx/api/loggingdemo/levels"

# Logging con datos estructurados
curl -X POST "https://localhost:7xxx/api/loggingdemo/structured" \
  -H "Content-Type: application/json" \
  -d '{"userId": 123, "name": "Test User", "email": "test@example.com"}'

# Logging con contexto y correlación
curl -X GET "https://localhost:7xxx/api/loggingdemo/context/456"
```

## Correcciones Implementadas

### Azure Service Bus
1. **Problema de recepción**: El processor ahora espera un tiempo específico antes de detenerse
2. **Configuración**: Connection string movido a configuración
3. **Logging**: Implementado logging estructurado con ILogger
4. **Manejo de errores**: Mejor manejo de excepciones y respuestas HTTP
5. **Validación**: Validación de parámetros de entrada
6. **Información de cola**: Nuevo endpoint para obtener métricas de la cola

### Sistema de Logging
1. **Serilog integrado**: Configuración completa con múltiples sinks
2. **Archivos de log**: Escritura simultánea a consola y archivos
3. **Rolling y retención**: Gestión automática de archivos antiguos
4. **Logging estructurado**: Soporte completo para objetos y correlación
5. **Gestión de logs**: Endpoints para visualizar y gestionar logs
6. **Performance**: Buffering y configuración optimizada

## Paquetes NuGet Utilizados

```xml
<!-- Azure Service Bus -->
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.18.1" />

<!-- Serilog -->
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
```

## Estructura del Proyecto

```
AzureApi/
??? Controllers/
?   ??? ServiceBusController.cs      # Azure Service Bus endpoints
?   ??? LogsController.cs            # Gestión de logs
?   ??? LoggingDemoController.cs     # Demostración de logging
??? Services/
?   ??? ServiceBusSenderExample.cs   # Envío de mensajes
?   ??? ServiceBusReceiverExample.cs # Recepción de mensajes
?   ??? ServiceBusQueueInfo.cs       # Información de cola
?   ??? LoggingDemoService.cs        # Servicios de demostración
??? Logs/                            # Archivos de log (auto-generados)
?   ??? log-YYYYMMDD.txt
?   ??? errors-YYYYMMDD.txt
??? appsettings.json                 # Configuración producción
??? appsettings.Development.json     # Configuración desarrollo
??? Program.cs                       # Configuración de la aplicación
??? README.md                        # Este archivo
??? SERILOG_GUIDE.md                # Guía detallada de Serilog
??? TESTING_LOGS.md                 # Script de pruebas de logging
```

## Notas de Seguridad

- No commits connection strings reales al repositorio
- Usa Azure Key Vault para producción
- Considera usar Managed Identity en Azure
- Los logs pueden contener información sensible - configura apropiadamente

## Monitoreo y Observabilidad

- Request logging automático con métricas de performance
- Correlación de logs con IDs únicos
- Logging estructurado para facilitar queries
- Separación de errores para alertas
- Health checks para verificar estado del sistema