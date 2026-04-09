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
- Shared global aliases wired through repository-level build props

## Shared Build Conventions

The repository uses two root-level files to enforce shared compile behavior across projects:

- `Directory.Build.props`
   - Automatically includes `GlobalUsings.Shared.cs` in every project under the repo.
   - This avoids repeating common aliases/usings in each project file.
- `GlobalUsings.Shared.cs`
   - Defines shared global aliases, currently:
      - `global using UserIdType = System.Ulid;`

Practical guidance:

- If you need a new cross-project alias, add it once in `GlobalUsings.Shared.cs`.
- Keep this file focused on broadly shared aliases to avoid polluting all projects with module-specific imports.

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
builder.Services.AddSharedKernelInfrastructure();
builder.Services.AddModules(builder.Configuration);
// Endpoint mapping can be explicit per module:
// app.MapModuleEndpoints<ClientesModule>();
```

`AddModules` reads `Modules` configuration entries and tries to load assemblies named `<ModuleName>.Module`.

### Explicit loading

```csharp
builder.Services.AddSharedKernelInfrastructure();
builder.Services.AddModule<ClientesModule>(builder.Configuration.GetSection("Modules:Clientes"));
app.MapModuleEndpoints<ClientesModule>();
```

## Inbox Writer Usage

When a module receives an integration event and needs reliable local processing, write it to inbox first:

```csharp
services.AddScoped<IInboxWriter<ClientesDbContext>, InboxWriter<ClientesModule, ClientesDbContext>>();
```

Example handler:

```csharp
public sealed class ClienteCriadoIntegrationEventHandler(
   IInboxWriter<ClientesDbContext> inboxWriter) : IEventHandler<ClienteCriadoIntegrationEvent>
{
   public async Task HandleAsync(ClienteCriadoIntegrationEvent integrationEvent, CancellationToken ct = default)
   {
      var result = await inboxWriter.WriteAsync(integrationEvent, ct);
      if (result.IsFailure)
         return;

      // Continue with module-specific logic.
   }
}
```

This flow works together with inbox workers (`InboxProcessingWorker`, `InboxRecoveryWorker`, `InboxCleanupWorker`) and `EventTypeRegistry` registration.

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
         "DatabaseProvider": "SqlServer",
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

`DatabaseProvider` supports `SqlServer` (default when omitted) and `Sqlite`.
For SQLite, use a connection string like `Data Source=./data/MyProject_Clientes.db`.

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
