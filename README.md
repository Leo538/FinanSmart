# FinanSmart

Aplicación de simulación de créditos e inversiones con Angular 20, ASP.NET Core 9 y PostgreSQL.

## Desarrollo local

Requisitos: .NET 9, Node.js con npm y PostgreSQL.

1. Copia `backend/FinanSmart.Api/appsettings.Development.example.json` como `backend/FinanSmart.Api/appsettings.Development.json`.
2. En el archivo local, configura `ConnectionStrings:DefaultConnection` con tu base de datos y `Jwt:Key` con una clave aleatoria larga. El archivo local está excluido de Git. También puedes usar las variables de entorno `ConnectionStrings__DefaultConnection` y `Jwt__Key`.
3. Aplica las migraciones: `dotnet ef database update --project backend/FinanSmart.Api`.
4. Inicia la API: `dotnet run --project backend/FinanSmart.Api --launch-profile http`. Escucha en `http://localhost:5246`.
5. En `frontend`, ejecuta `npm ci` y `npm start`. La web de desarrollo abre en `http://localhost:4200` y llama a la API local.

La API requiere PostgreSQL y una clave JWT para funcionar; no se incluyen credenciales en el repositorio. Si falta alguna configuración obligatoria, muestra un error al iniciar. El directorio `Storage` se crea automáticamente y no se versiona. Solo los logos institucionales se sirven públicamente; los documentos de solicitudes se entregan mediante los endpoints autorizados.

## Producción

Ejecuta `npm run build` en `frontend`. La compilación de producción usa rutas relativas `/api`, por lo que la API debe estar en el mismo origen o detrás de un proxy que envíe `/api` y `/storage/institution-logos` al backend. Configura la conexión a PostgreSQL y `Jwt__Key` como secretos del entorno de despliegue. No uses `appsettings.Development.json` en producción.

Los archivos generados (`bin`, `obj`, `dist`, `node_modules`), la configuración local y `backend/FinanSmart.Api/Storage` están excluidos de Git. Si este repositorio se publicó antes de esta limpieza, los documentos que ya estuvieran en commits anteriores seguirán en el historial: purgar ese historial requiere una operación de Git separada y coordinada antes de publicarlo.
