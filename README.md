# FinanSmart

FinanSmart es una plataforma para explorar opciones financieras antes de tomar una decisión. Permite simular créditos con los sistemas Francés y Alemán, comparar cuotas y descargar la tabla de amortización en PDF. También permite proyectar inversiones, iniciar una solicitud y seguir su estado. Incluye una página informativa, acceso para clientes y un panel de administración para configurar productos, tasas e identidad visual.

Está desarrollada con Angular 20, ASP.NET Core 9 y PostgreSQL.

## Antes de iniciar

Necesitas .NET 9, Node.js con npm y PostgreSQL. Copia `backend/FinanSmart.Api/appsettings.Development.example.json` a `backend/FinanSmart.Api/appsettings.Development.json` y configura allí la conexión a PostgreSQL y `Jwt:Key`. Ese archivo local y `Storage` no se suben al repositorio.

Aplica las migraciones desde la raíz del proyecto:

```powershell
dotnet ef database update --project .\backend\FinanSmart.Api
```

## Iniciar el backend

Abre una terminal en la raíz del proyecto:

```powershell
cd .\backend
dotnet run --project .\FinanSmart.Api
```

La API estará en `http://localhost:5246`.

## Iniciar el frontend

Abre otra terminal en la raíz del proyecto:

```powershell
cd .\frontend
npm ci
ng serve
```

Abre `http://localhost:4200`. Si `ng` no está disponible en tu terminal, usa `npx ng serve`.

Para producción, `npm run build` genera el frontend con rutas `/api` relativas al mismo origen. Si el repositorio ya se publicó antes de excluir la configuración local y los documentos, recuerda que esos archivos pueden seguir en el historial antiguo de Git.
