# ModularAPITemplate

A .NET 10 template set for building modular Web APIs with Clean Architecture. Modules stay isolated and communicate through events, while common infrastructure lives in SharedKernel.

## What You Get

This repository ships two templates:

| Template | Short name | Purpose |
|---|---|---|
| Modular API Template | `modularapi` | Creates a new solution skeleton (`Host`, `SharedKernel`, module/test placeholders) |
| Modular API - Module | `modularapi-module` | Scaffolds one module + one test project for an existing solution |

## Features

- Modular architecture (module-owned DbContext, endpoints, and DI registration)
- Clean Architecture structure per module (Domain, Application, Infrastructure, Endpoints)
- Request/handler pipeline via SharedKernel dispatcher abstractions
- Integration events through `IEventBus` with outbox/inbox processing support
- OpenAPI per module (Scalar or Swagger UI in development)
- Request context (`IRequestContext`) and result pattern (`Result`, `Result<T>`)
- Background worker base + inbox/outbox workers

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Install Templates

From the repository root:

```bash
dotnet new install .
```

Verify:

```bash
dotnet new list modularapi
```

## Quick Start

### 1. Create a new solution

```bash
dotnet new modularapi -n MyProject -o MyProject
cd MyProject
```

Current output (minimal skeleton):

```text
MyProject/
|- src/
|  |- Host/
|  |- SharedKernel/
|  `- Modules/
|- tests/
|  `- Modules/
`- MyProject.slnx
```

### 2. Build and run

```bash
dotnet build
dotnet run --project src/Host/MyProject.Host.csproj
```

In development, OpenAPI UI is available at:

- `/scalar` when `OpenApi:UI = Scalar`
- `/swagger` when `OpenApi:UI = Swagger`

### 3. Add a module

From solution root:

```bash
dotnet new modularapi-module -n Clientes --SolutionPrefix MyProject
```

Then finish setup:

1. Add projects to solution folders:
    ```bash
    dotnet sln add src/Modules/Clientes/MyProject.Modules.Clientes.csproj --solution-folder Modules
    dotnet sln add tests/Modules/Clientes/MyProject.Modules.Clientes.Tests.csproj --solution-folder Tests
    ```
2. Add module reference to Host:
    ```bash
    dotnet add src/Host/MyProject.Host.csproj reference src/Modules/Clientes/MyProject.Modules.Clientes.csproj
    ```
3. Register the module in `Program.cs`.
4. Configure module settings in `appsettings.json`.
5. Create migrations with EF CLI (do not auto-run migrations at startup).

## Module Registration Patterns

### Config-driven loading (default Host style)

```csharp
builder.Services.AddModules(builder.Configuration);
// Endpoint mapping can be explicit per module:
// app.MapModuleEndpoints<ClientesModule>();
```

`AddModules` reads `Modules` configuration entries and tries to load assemblies named `<ModuleName>.Module`.

### Explicit loading

```csharp
builder.Services.AddModule<ClientesModule>(builder.Configuration.GetSection("Modules:Clientes"));
app.MapModuleEndpoints<ClientesModule>();
```

## Configuration

### OpenAPI UI

```json
{
   "OpenApi": {
      "UI": "Scalar"
   }
}
```

Supported values: `Scalar` (default) and `Swagger`.

### Module settings (example)

```json
{
   "Modules": {
      "Clientes": {
         "ConnectionString": "Server=localhost;Database=MyProject_Clientes;Trusted_Connection=true;TrustServerCertificate=true",
         "Outbox": {
            "Enabled": true,
            "IntervalMilliseconds": 1000,
            "BatchSize": 50
         },
         "Inbox": {
            "Enabled": true,
            "IntervalMilliseconds": 1000,
            "BatchSize": 50
         }
      }
   }
}
```

Use environment variables for secrets in production.

## Template Notes

- The main template intentionally excludes `templates/**`, repo metadata, and docs from generated projects.
- The module template scaffolds structure and infrastructure entry points; business entities/use cases/endpoints are intentionally left for implementation.

## Architecture Reference

See [architecture.md](architecture.md) for the full architecture source of truth.

## AI-Assisted Development

Project-specific Copilot guidance lives in [.copilot/.description.txt](.copilot/.description.txt). Update the `<project_details>` section when starting a real project from this template.

## Uninstall Templates

```bash
dotnet new uninstall <path-to-this-repo>
```

## License

MIT
