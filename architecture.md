# Architecture - ModularAPITemplate

> **This is the source of truth.**

## Overview

ModularAPITemplate provides two .NET templates:

1. `modularapi` (solution template)
2. `modularapi-module` (module template for an existing solution)

The architecture is modular: each module owns its own persistence and HTTP surface, while shared cross-cutting concerns live in `SharedKernel`.

## Template Inventory

### Main template (`modularapi`)

- Creates a new solution skeleton with:
  - `src/Host`
  - `src/SharedKernel`
  - `src/Modules` (placeholder)
  - `tests/Modules` (placeholder)
- Does **not** include prebuilt sample modules by default.
- Uses template identity `ModularAPITemplate.Solution` and short name `modularapi`.
- Replaces `-` with `.` in the project root name for namespaces/file names.
- Excludes from generated output: `templates/**`, `.git/**`, `README.md`, `LICENSE.txt`, build artifacts, and editor-specific files.

### Module template (`modularapi-module`)

- Scaffolds one module under `src/Modules/<ModuleName>` and one test project under `tests/Modules/<ModuleName>`.
- Uses template identity `ModularAPITemplate.Module` and short name `modularapi-module`.
- Supports `--SolutionPrefix` to replace namespace/project prefix (default: `ModularAPITemplate`).
- Generates a lowercased schema token from module name (`nomemodulo_schema` replacement).
- Produces structure only (empty folders/placeholders) for most business code areas.

## Generated Solution Structure (`modularapi`)

```text
src/
|- Host/                         -> Web app entry point (Program.cs)
|- SharedKernel/                 -> Shared abstractions/infrastructure
`- Modules/                      -> Placeholder for future modules

tests/
`- Modules/                      -> Placeholder for future module tests
```

## Generated Module Structure (`modularapi-module`)

```text
src/Modules/<ModuleName>/
|- <Prefix>.Modules.<ModuleName>.csproj
|- <ModuleName>Module.cs
|- Application/
|  |- DTOs/
|  `- UseCases/
|- Domain/
|  |- Entities/
|  `- Events/
|- Endpoints/
`- Infrastructure/
   `- Persistence/
      `- <ModuleName>DbContext.cs

tests/Modules/<ModuleName>/
|- <Prefix>.Modules.<ModuleName>.Tests.csproj
|- Application/UseCases/
`- Domain/
```

## Core Concepts

### SharedKernel

SharedKernel centralizes reusable abstractions and infrastructure, including:

- Request pipeline abstractions: `IRequest`, `IRequestHandler`, `IDispatcher`
- Event abstractions: `IEvent`, `IEventBus`, `IEventHandler<T>`, `DomainEvent`, `IntegrationEvent`
- Request context: `IRequestContext`
- Workers: `BaseWorker`, inbox/outbox workers
- Persistence helpers:
  - `IBaseDbContext`
  - `AuditSaveChangesInterceptor`
  - outbox/inbox entities + EF configuration extensions
  - soft-delete global query filter extension
- Utility models: `Result`, `Result<T>`, `PagedResult<T>`

### Repository-level compile conventions

The solution uses repository-level build wiring for shared aliases/usings:

- `Directory.Build.props`
  - Includes `GlobalUsings.Shared.cs` into all projects in this repository.
  - Ensures consistent shared type aliases without per-project setup.
- `GlobalUsings.Shared.cs`
  - Contains global aliases used by multiple projects.
  - Current alias:
    - `global using UserIdType = System.Ulid;`

Rule of thumb:

- Add only genuinely cross-cutting aliases here.
- Keep module-specific using directives inside each module project.

### Module Isolation

- Modules should be self-contained and own:
  - DbContext
  - endpoint mappings
  - DI registration
  - schema
- Modules should **not** reference each other directly.
- Cross-module communication should happen through integration events.

### Allowed Dependencies

```text
Host -> SharedKernel, Modules
Module -> SharedKernel (only)
SharedKernel -> external packages only
```

## Registration and Discovery

### `IModule` contract

Every module implements `IModule` with:

- `ModuleName`
- `RegisterServices(IServiceCollection, IConfiguration)`
- `MapEndpoints(IEndpointRouteBuilder)`

### Service registration

`ModuleExtensions` provides:

- `AddModule<TModule>(services, configuration)` for explicit registration
- `AddModules(services, configuration)` for config-driven registration

What `AddModule` does:

- registers shared services once (`IRequestContext`, `AuditSaveChangesInterceptor`, `IDispatcher`)
- registers module `InboxConfiguration<TModule>` and `OutboxConfiguration<TModule>`
- tracks OpenAPI document names via `OpenApiModuleTracker`
- registers one OpenAPI document per module
- scans module assembly for:
  - `IEventHandler<T>`
  - `IRequestHandler<TRequest>`
  - `IRequestHandler<TRequest, TResponse>`

What `AddModules` does:

- reads `Modules` section from configuration
- tries to load each module assembly by name pattern `<ModuleName>.Module`
- finds the module type implementing `IModule`
- invokes `AddModule<TModule>` via reflection

### Endpoint mapping

`AddModules` and `AddModule` register services, but endpoint mapping is still done through `MapModuleEndpoints<TModule>()`.

## OpenAPI

- Host supports Scalar and Swagger UI (`OpenApi:UI` setting).
- Each module gets a dedicated OpenAPI document named by `ModuleName`.
- OpenAPI inclusion uses case-insensitive group comparison to avoid document/group casing mismatch.
- Module endpoints should call `.WithGroupName(ModuleName)` to appear in that module document.

## Persistence

- Module templates currently generate `DbContext` classes that implement `IBaseDbContext` directly.
- SharedKernel persistence features are added through:
  - model builder extensions (`ApplySoftDeleteQueryFilter`, `ConfigureOutboxMessage`, `ConfigureInboxMessage`)
  - audit interceptor (`AuditSaveChangesInterceptor`)
- Each module should define a dedicated schema (`HasDefaultSchema(...)`).
- Outbox/inbox tables are expected in module contexts via `OutboxMessages` and `InboxMessages`.

### Migrations

Do not run migrations automatically. Use EF CLI from source control workflow:

```bash
dotnet ef migrations add <Name> -p <ModuleProject> -s <HostProject>
```

## Events and Messaging

### Integration events

- Publish via `IEventBus.PublishAsync(...)`.
- Default bus is `InProcessEventBus<TContext>`.
- Integration events are persisted via `IIntegrationEventPublisher<TContext>` into outbox.
- `EventTypeRegistry` resolves CLR type names during inbox/outbox processing.

### Inbox writer usage (`IInboxWriter<TContext>`)

Use `IInboxWriter<TContext>` when a module receives an integration event and needs reliable local processing.

Register it in the module:

```csharp
services.AddScoped<IInboxWriter<MyModuleDbContext>, InboxWriter<MyModule, MyModuleDbContext>>();
```

Typical flow in an integration event handler:

```csharp
public sealed class PedidoCriadoIntegrationEventHandler(
  IInboxWriter<ShopDbContext> inboxWriter) : IEventHandler<PedidoCriadoIntegrationEvent>
{
  public async Task HandleAsync(PedidoCriadoIntegrationEvent integrationEvent, CancellationToken ct = default)
  {
    var result = await inboxWriter.WriteAsync(integrationEvent, ct);

    if (result.IsFailure)
    {
      // Log and decide retry policy according to module requirements.
      return;
    }

    // Continue with module-specific processing.
  }
}
```

Notes:

- Inbox messages are stored in `IBaseDbContext.InboxMessages`.
- `InboxProcessingWorker` handles asynchronous processing of persisted inbox messages.
- Keep `EventTypeRegistry.RegisterFromAssembly(...)` configured so message types can be resolved.

### Workers

- `BaseWorker` provides structured loop, interval handling, logging, and error handling.
- Per-module workers are commonly registered:
  - `InboxProcessingWorker`
  - `InboxRecoveryWorker`
  - `InboxCleanupWorker`
  - `OutboxProcessingWorker`
  - `OutboxRecoveryWorker`
  - `OutboxCleanupWorker`

## Configuration Model

`OutboxConfiguration<TModule>` and `InboxConfiguration<TModule>` read settings under:

```json
"Modules": {
  "<ModuleName>": {
    "Outbox": {
      "Enabled": true,
      "IntervalMilliseconds": 1000,
      "BatchSize": 50,
      "PartitionStart": 0,
      "PartitionEnd": 63,
      "RecoveryThresholdSeconds": 60,
      "CleanupThresholdDays": 7,
      "MaxRetryAttempts": 3,
      "BacklogWarningThreshold": 1000,
      "BacklogWarningCooldownSeconds": 300
    },
    "Inbox": {
      "Enabled": true,
      "IntervalMilliseconds": 1000,
      "BatchSize": 50,
      "PartitionStart": 0,
      "PartitionEnd": 63,
      "RecoveryThresholdSeconds": 60,
      "CleanupThresholdDays": 7,
      "MaxRetryAttempts": 3,
      "BacklogWarningThreshold": 1000,
      "BacklogWarningCooldownSeconds": 300
    }
  }
}
```

For generated module DbContexts, connection string is currently read from the module configuration value `ConnectionString` in `RegisterServices`.

## How to Add a New Module

### Recommended (CLI)

```bash
dotnet new modularapi-module -n <ModuleName> --SolutionPrefix <SolutionPrefix>
```

### Post-generation steps

1. Add generated projects to solution folders:
   ```bash
   dotnet sln add src/Modules/<ModuleName>/<Prefix>.Modules.<ModuleName>.csproj --solution-folder Modules
   dotnet sln add tests/Modules/<ModuleName>/<Prefix>.Modules.<ModuleName>.Tests.csproj --solution-folder Tests
   ```
2. Add module project reference to Host project.
3. Register services and map endpoints in `Program.cs`.
4. Add module configuration in `appsettings.json` (at minimum module activation/config values expected by your module).
5. Create module migrations using EF CLI.

## Testing

Module test template includes:

- `xUnit`
- `NSubstitute`
- `Microsoft.EntityFrameworkCore.InMemory`
- `coverlet.collector`

Recommended coverage:

- domain rules
- use case handlers
- endpoint behavior (when added)
- integration event handlers

## Naming and Language Conventions

- Domain names/entities/business terms: pt-BR
- Logging/infrastructure messages: English

## Dependency Diagram

```text
+---------------------------------------------+
| Host                                        |
| Program.cs: modules + HTTP pipeline + docs  |
+------------------------+--------------------+
                         |
                         v
+---------------------------------------------+
| SharedKernel                                |
| IRequest/Dispatcher | Events | Persistence  |
| BaseWorker          | Result<T>             |
+------------------------^--------------------+
                         |
                         |
              +----------+-----------+
              |      Module N        |
              | Domain/Application   |
              | Infrastructure       |
              | Endpoints            |
              +----------------------+
```
