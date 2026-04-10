# NomeModulo static files module

This module serves static assets from `wwwroot` using the route configured in `Modules:NomeModulo:RoutePath`.

## Configuration

```json
{
  "Modules": {
    "NomeModulo": {
      "RoutePath": "/nome-modulo"
    }
  }
}
```

- `RoutePath` defaults to `/<module-name-lowercase>`.
- Use `"/"` to serve this module at root.
- Only one module should use `"/"`.

## Development modes

### 1. Host serves built files (single URL)

Generate your frontend artifacts into `src/Modules/NomeModulo/wwwroot`.

Examples:

```bash
npm run build
# or your tool's watch command writing to ../wwwroot
```

In another terminal from solution root:

```bash
dotnet run --project src/Host/ModularAPITemplate.Host.csproj
```

### 2. Frontend dev server separately (HMR)

Run your frontend dev server independently (for example Vite, Angular, React, Vue, etc.):

```bash
npm start
```

- Frontend runs on its own local URL/port.
- Host can run independently for API endpoints.
- This mode does not pass through the module route path; it is intended for frontend-only iteration.
