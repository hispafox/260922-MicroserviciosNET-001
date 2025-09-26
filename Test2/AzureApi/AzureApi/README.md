# Azure Service Bus API

Este proyecto demuestra c�mo usar Azure Service Bus para enviar y recibir mensajes usando .NET 8 con Serilog para logging completo.

## Configuraci�n

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
- **POST** `/api/servicebus/send` - Env�a mensajes de prueba a la cola
  - Query parameter: `messageCount` (1-100, default: 5)
- **POST** `/api/servicebus/receive` - Recibe mensajes de la cola por un tiempo determinado
  - Query parameter: `timeoutSeconds` (5-300, default: 30)
- **GET** `/api/servicebus/queue-info` - Obtiene informaci�n detallada sobre la cola
- **GET** `/api/servicebus/health` - Verifica el estado de la configuraci�n

### Gesti�n de Logs
- **GET** `/api/logs/health` - Verifica el estado del sistema de logging
- **POST** `/api/logs/test` - Genera logs de prueba en todos los niveles
- **GET** `/api/logs/files` - Lista archivos de log disponibles con informaci�n de tama�o
- **GET** `/api/logs/files/{fileName}/tail` - Lee las �ltimas N l�neas de un archivo de log
- **DELETE** `/api/logs/cleanup` - Limpia logs antiguos manualmente

### Demostraci�n de Logging
- **GET** `/api/loggingdemo/levels` - Demuestra diferentes niveles de logging
- **POST** `/api/loggingdemo/structured` - Demuestra logging estructurado con objetos
- **GET** `/api/loggingdemo/context/{userId}` - Demuestra logging con contexto y scopes
- **GET** `/api/loggingdemo/error` - Simula errores para probar logging de excepciones
- **GET** `/api/loggingdemo/performance` - Demuestra logging de performance con m�tricas

## Sistema de Logging

### Archivos de Log Generados
- `Logs/log-YYYYMMDD.txt` - Todos los logs del d�a
- `Logs/errors-YYYYMMDD.txt` - Solo errores y cr�ticos
- `Logs/dev-log-YYYYMMDD.txt` - Logs de desarrollo (m�s detallados)

### Caracter�sticas del Logging
- ? **Salida dual**: Consola y archivos simult�neamente
- ? **Rolling diario**: Nuevos archivos cada d�a
- ? **Retenci�n autom�tica**: 7 d�as logs generales, 30 d�as errores
- ? **Logging estructurado**: Par�metros con nombre y objetos serializados
- ? **Request logging**: Autom�tico para todas las peticiones HTTP
- ? **Correlaci�n**: IDs �nicos para rastrear operaciones
- ? **Configuraci�n flexible**: Por ambiente y namespace
- ? **Performance**: Buffering y flush autom�tico

## Ejemplos de Uso

### Service Bus
```bash
# Enviar 10 mensajes
curl -X POST "https://localhost:7xxx/api/servicebus/send?messageCount=10"

# Recibir mensajes por 60 segundos
curl -X POST "https://localhost:7xxx/api/servicebus/receive?timeoutSeconds=60"

# Obtener informaci�n de la cola
curl -X GET "https://localhost:7xxx/api/servicebus/queue-info"
```

### Logging
```bash
# Generar logs de prueba
curl -X POST "https://localhost:7xxx/api/logs/test"

# Ver archivos de log
curl -X GET "https://localhost:7xxx/api/logs/files"

# Leer �ltimas 20 l�neas del log principal
curl -X GET "https://localhost:7xxx/api/logs/files/log-20250109.txt/tail?lines=20"

# Verificar estado del logging
curl -X GET "https://localhost:7xxx/api/logs/health"
```

### Demostraci�n de Logging Estructurado
```bash
# Probar diferentes niveles
curl -X GET "https://localhost:7xxx/api/loggingdemo/levels"

# Logging con datos estructurados
curl -X POST "https://localhost:7xxx/api/loggingdemo/structured" \
  -H "Content-Type: application/json" \
  -d '{"userId": 123, "name": "Test User", "email": "test@example.com"}'

# Logging con contexto y correlaci�n
curl -X GET "https://localhost:7xxx/api/loggingdemo/context/456"
```

## Correcciones Implementadas

### Azure Service Bus
1. **Problema de recepci�n**: El processor ahora espera un tiempo espec�fico antes de detenerse
2. **Configuraci�n**: Connection string movido a configuraci�n
3. **Logging**: Implementado logging estructurado con ILogger
4. **Manejo de errores**: Mejor manejo de excepciones y respuestas HTTP
5. **Validaci�n**: Validaci�n de par�metros de entrada
6. **Informaci�n de cola**: Nuevo endpoint para obtener m�tricas de la cola

### Sistema de Logging
1. **Serilog integrado**: Configuraci�n completa con m�ltiples sinks
2. **Archivos de log**: Escritura simult�nea a consola y archivos
3. **Rolling y retenci�n**: Gesti�n autom�tica de archivos antiguos
4. **Logging estructurado**: Soporte completo para objetos y correlaci�n
5. **Gesti�n de logs**: Endpoints para visualizar y gestionar logs
6. **Performance**: Buffering y configuraci�n optimizada

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
?   ??? LogsController.cs            # Gesti�n de logs
?   ??? LoggingDemoController.cs     # Demostraci�n de logging
??? Services/
?   ??? ServiceBusSenderExample.cs   # Env�o de mensajes
?   ??? ServiceBusReceiverExample.cs # Recepci�n de mensajes
?   ??? ServiceBusQueueInfo.cs       # Informaci�n de cola
?   ??? LoggingDemoService.cs        # Servicios de demostraci�n
??? Logs/                            # Archivos de log (auto-generados)
?   ??? log-YYYYMMDD.txt
?   ??? errors-YYYYMMDD.txt
??? appsettings.json                 # Configuraci�n producci�n
??? appsettings.Development.json     # Configuraci�n desarrollo
??? Program.cs                       # Configuraci�n de la aplicaci�n
??? README.md                        # Este archivo
??? SERILOG_GUIDE.md                # Gu�a detallada de Serilog
??? TESTING_LOGS.md                 # Script de pruebas de logging
```

## Notas de Seguridad

- No commits connection strings reales al repositorio
- Usa Azure Key Vault para producci�n
- Considera usar Managed Identity en Azure
- Los logs pueden contener informaci�n sensible - configura apropiadamente

## Monitoreo y Observabilidad

- Request logging autom�tico con m�tricas de performance
- Correlaci�n de logs con IDs �nicos
- Logging estructurado para facilitar queries
- Separaci�n de errores para alertas
- Health checks para verificar estado del sistema