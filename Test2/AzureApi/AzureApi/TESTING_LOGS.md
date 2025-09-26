# Script de Prueba de Logs para AzureApi

## Cómo probar que los logs se escriben correctamente

### 1. Verificar Estado del Sistema de Logs
```bash
curl -X GET "https://localhost:7xxx/api/logs/health"
```

### 2. Generar Logs de Prueba
```bash
curl -X POST "https://localhost:7xxx/api/logs/test"
```

### 3. Ver Archivos de Log Disponibles
```bash
curl -X GET "https://localhost:7xxx/api/logs/files"
```

### 4. Leer las Últimas 20 Líneas del Log Principal
```bash
curl -X GET "https://localhost:7xxx/api/logs/files/log-20250109.txt/tail?lines=20"
```

### 5. Leer las Últimas 10 Líneas del Log de Errores
```bash
curl -X GET "https://localhost:7xxx/api/logs/files/errors-20250109.txt/tail?lines=10"
```

### 6. Probar Endpoints de Service Bus (generan logs automáticamente)
```bash
# Verificar salud
curl -X GET "https://localhost:7xxx/api/servicebus/health"

# Enviar mensajes (genera logs)
curl -X POST "https://localhost:7xxx/api/servicebus/send?messageCount=3"

# Recibir mensajes (genera logs)
curl -X POST "https://localhost:7xxx/api/servicebus/receive?timeoutSeconds=10"
```

### 7. Probar Logs de Demostración
```bash
# Niveles de logging
curl -X GET "https://localhost:7xxx/api/loggingdemo/levels"

# Logging estructurado
curl -X POST "https://localhost:7xxx/api/loggingdemo/structured" \
  -H "Content-Type: application/json" \
  -d '{"userId": 123, "name": "Test User", "email": "test@example.com"}'

# Logging con contexto
curl -X GET "https://localhost:7xxx/api/loggingdemo/context/456"

# Generar error (aparecerá en errors-.txt)
curl -X GET "https://localhost:7xxx/api/loggingdemo/error"
```

## Archivos de Log Esperados

Después de ejecutar las pruebas, deberías ver estos archivos en la carpeta `Logs/`:

- `log-YYYYMMDD.txt` - Todos los logs del día
- `errors-YYYYMMDD.txt` - Solo errores y críticos del día

## Ejemplo de Contenido de Log

### Archivo: log-20250109.txt
```
2025-01-09 10:30:15.123 +00:00 [INF] Aplicación iniciada correctamente en Development <s:Program> <id:> <app:AzureApi>
2025-01-09 10:30:20.456 +00:00 [INF] HTTP GET /api/logs/health responded 200 in 45.2000 ms <s:Microsoft.AspNetCore.Hosting.Diagnostics> <id:>
2025-01-09 10:30:25.789 +00:00 [INF] Health check ejecutado desde GET /api/logs/health <s:AzureApi.Controllers.LogsController> <id:>
```

### Archivo: errors-20250109.txt
```
2025-01-09 10:35:10.123 +00:00 [ERR] Mensaje de ERROR - error simulado para pruebas <s:AzureApi.Controllers.LogsController> <id:> <app:AzureApi>
2025-01-09 10:35:15.456 +00:00 [CRT] Mensaje de CRITICAL - error crítico simulado <s:AzureApi.Controllers.LogsController> <id:> <app:AzureApi>
```

## Verificación Manual

También puedes verificar manualmente navegando a la carpeta `Logs/` en el directorio de tu aplicación:

```
AzureApi/
??? Logs/
?   ??? log-20250109.txt
?   ??? errors-20250109.txt
?   ??? (archivos de días anteriores)
??? Program.cs
??? ...
```

## Características de los Logs

- ? **Rolling diario**: Se crea un nuevo archivo cada día
- ? **Retención automática**: Se mantienen 7 días de logs generales y 30 días de errores
- ? **Buffering**: Los logs se escriben en lotes para mejor performance
- ? **Flush automático**: Se vacía el buffer cada segundo
- ? **Formato estructurado**: Incluye timestamp, nivel, mensaje, contexto, correlación y aplicación
- ? **Separación de errores**: Errores y críticos van a archivo separado
- ? **Logging de requests HTTP**: Todos los requests se loguean automáticamente

## Solución de Problemas

### Si no se crean archivos de log:

1. Verificar permisos de escritura en el directorio
2. Ejecutar `/api/logs/health` para verificar estado
3. Generar logs manualmente con `/api/logs/test`
4. Revisar logs de consola para errores

### Si los archivos están vacíos:

1. Verificar configuración de niveles de log
2. Asegurar que `flushToDiskInterval` esté configurado
3. Esperar unos segundos para el flush automático
4. Revisar configuración en `appsettings.json`