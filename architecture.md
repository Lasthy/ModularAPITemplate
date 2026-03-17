# Architecture — ModularAPITemplate

> **This is the source of truth.** A Portuguese version is available at [.ARCHITECTURE.pt-BR.md](.ARCHITECTURE.pt-BR.md).

## Overview

ModularAPITemplate is a template for building modular Web APIs on .NET 10.
Each module is self-contained (business rules, persistence, endpoints) and communicates with other modules via integration events.
Shared infrastructure (events, request pipeline, outbox, logging helpers, etc.) lives in the **SharedKernel**.

## Project Structure

```
src/
├── Host/                                  → Web project (entry point)
├── SharedKernel/                          → Shared abstractions and infrastructure
└── Modules/
    └── <ModuleName>/                      → Domain module (csproj at folder root)

tests/
└── Modules/
    └── <ModuleName>/                      → Module tests (csproj at folder root)
```

## Module Layers

Each module follows Clean Architecture internally:

```
ModularAPITemplate.Modules.<ModuleName>/
├── Domain/
│   ├── Entities/       → Entities and aggregates
│   └── Events/         → Domain events
├── Application/
│   ├── DTOs/           → Data Transfer Objects and mappings
│   └── UseCases/       → Use cases (Commands/Queries + Handlers via MediatR)
├── Infrastructure/
│   └── Persistence/    → DbContext, EF configurations, outbox tables
├── Endpoints/          → Minimal API endpoint mappings
└── <ModuleName>Module.cs → DI registration + endpoint wiring (implements IModule)
```

## Core Concepts

### SharedKernel
- Central shared abstractions and infrastructure used across all modules.
- Contains:
  - `IEventBus`, `IEventHandler<T>`, `DomainEvent`, `IntegrationEvent`
  - `IRequestContext` (authenticated user context)
  - `Result<T>` for operation results
  - `BaseWorker`, background worker helpers
  - Persistence helpers: `BaseDbContext`, outbox support, audit interceptors, soft-delete filters

### Module Isolation
- Each module is self-contained: it owns its own DbContext, endpoints, DI registration, and schema.
- Modules **do not reference each other directly**.
- Cross-module communication happens only via **integration events**.

### Allowed Dependencies
```
Host → SharedKernel, Modules
Module → SharedKernel (only)
SharedKernel → NuGet packages (no module dependencies)
```

### Module Registration & OpenAPI
- Each module implements `IModule`.
- `ModuleName` is used as the OpenAPI document identifier.
- The Host can register modules either explicitly or via configuration:
  - Explicit (code):
    ```csharp
    builder.Services.AddModule<MyModule>(builder.Configuration);
    app.MapModuleEndpoints<MyModule>();
    ```
  - Config-driven (recommended for templates):
    ```csharp
    builder.Services.AddModules(builder.Configuration);
    app.MapModuleEndpoints<MyModule>(); // or map dynamically based on config
    ```
- When using configuration, modules are enabled by adding them to the `Modules` section
  of `appsettings.json` (the values can be empty objects):
  ```json
  "Modules": {
    "NomeModulo": {},
    "AnotherModule": {}
  }
  ```
- `AddModule` / `AddModules`:
  - registers shared kernel services once (request context, audit interceptor, dispatcher)
  - registers the module's OpenAPI document automatically
  - scans the module assembly for event/request handlers and registers them

### Persistence
- Every module uses its own `DbContext` (derived from `BaseDbContext`).
- `BaseDbContext` automatically dispatches domain events before saving.
- Shared kernel provides: soft-delete query filters, outbox persistence mapping, audit field interception.
- Each module should set a dedicated schema to avoid collision.
- **Migration rule:** Do NOT run migrations automatically. Use EF CLI:
  ```bash
  dotnet ef migrations add <Name> -p <Project> -s <Host>
  ```

### Use Cases
- Implemented as **Commands** (write) or **Queries** (read) using MediatR-style handlers.
- Use `Result<T>` for safe success/failure flows.

### Domain Events
- Entities raise domain events via `RaiseDomainEvent()`.
- `BaseDbContext` dispatches domain events before saving.
- Use `DomainEvent` to model domain-level event data.

### Integration Events (Cross-module)
- Use `IEventBus.PublishAsync()` to publish integration events.
- Default implementation is `InProcessEventBus` (in-process handler invocation).
- Can be swapped for external brokers (RabbitMQ/Kafka/etc).
- Outbox pattern is used to persist integration events reliably.

### Cross-module Communication Patterns
- Modules do **not** reference each other directly.
- Source module raises a domain event → handler publishes integration event → other modules consume integration event.
- Shared contracts (integration events, role constants, etc.) live in SharedKernel.
- Example flow:
  - Identity: `UserCreated` (domain event) → publishes `UserCreatedIntegrationEvent`.
  - Shop: handles event, creates `Cliente`.
  - Sales: handles event, creates `Vendedor`.
- Consistency is eventual: consumers may fail without affecting the source transaction.

### Event Type Resolution & Outbox
- Integration events are stored in the outbox table as JSON along with their CLR type name.
- `EventTypeRegistry` resolves the type when processing outbox messages.
- Outbox workers (processing/recovery/cleanup) are registered per module.

### Request Context (IRequestContext)
- Provides authenticated user info: `UserId`, `UserName`, `Roles`, `Claims`, etc.
- `RequestContext` reads from `HttpContext.User`.
- Registered automatically by `AddModule`.
- Modules can extend the context for module-specific needs.

### Workers
- Workers inherit from `BaseWorker` (a `BackgroundService` subclass).
- Standardized error handling and logging.
- Use `IServiceScopeFactory` to resolve scoped services.

### Endpoints and OpenAPI
- Use Minimal APIs grouped by module: `app.MapGroup(...)`.
- Use `.WithGroupName(ModuleName)` to associate endpoints with the module's OpenAPI document.
- Document endpoints using `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`, `.WithTags()`.
- OpenAPI UI is configurable via `appsettings.json`:
  ```json
  "OpenApi": { "UI": "Scalar" }
  ```

### Naming Conventions
- Domain concepts and entities: **pt-BR**.
- Logging and infrastructure messages: **English**.

### Testing
- Each module should have a test project.
- Use `NSubstitute` for mocking.
- Add tests for business rules (domain) and use case behavior (application).

### Configuration
- Use environment variables for secrets in production.
- Use `appsettings.Development.json` in development.
- Do not commit secrets.

## Module Template (current structure)

Template folder: `templates/module`

```
templates/module/
├── src/
│   └── Modules/
│       └── NomeModulo/
│           ├── NomeModuloModule.cs
│           ├── Application/
│           │   ├── DTOs/
│           │   └── UseCases/
│           ├── Domain/
│           │   ├── Entities/
│           │   └── Events/
│           ├── Endpoints/
│           └── Infrastructure/
│               └── Persistence/NomeModuloDbContext.cs
└── tests/
    └── Modules/
        └── NomeModulo/
            └── ...
```

## How to Create a New Module

### Using the template (recommended):
```bash
# From the solution root:
dotnet new modularapi-module -n <ModuleName> --SolutionPrefix <SolutionPrefix>
```

### Post-creation steps:
1. Add generated projects to the solution:
   ```bash
   dotnet sln add src/Modules/<ModuleName>/<Prefix>.Modules.<ModuleName>.csproj --solution-folder Modules
   dotnet sln add tests/Modules/<ModuleName>/<Prefix>.Modules.<ModuleName>.Tests.csproj --solution-folder Tests
   ```
2. Add module reference into `Host.csproj`.
3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddModule<ModuleNameModule>(builder.Configuration);
   app.MapModuleEndpoints<ModuleNameModule>();
   ```
4. Add connection string to `appsettings.json`:
   ```json
   "ConnectionStrings": { "ModuleNameDb": "..." }
   ```

### Manual (without template):
1. Create the project: `dotnet new classlib -n <Prefix>.Modules.<Name>`
2. Add reference to SharedKernel and ASP.NET Core `FrameworkReference`.
3. Create folder structure: `Domain/`, `Application/`, `Infrastructure/`, `Endpoints/`.
4. Implement `IModule` in `<Name>Module.cs`.
5. Register in the Host: `AddModule<NameModule>()`, `MapModuleEndpoints<NameModule>()`.
6. Create test project and add to the solution.
7. Add connection string to `appsettings.json`.

## Dependency Diagram

```
┌──────────────────────────────────────────────┐
│                    Host                      │
│  (Program.cs — registers modules & pipeline) │
└──────┬───────────────────┬───────────────────┘
       │                   │
       ▼                   ▼
┌──────────────┐   ┌────────────────────┐
│ SharedKernel │◄──│  Produtos Module   │
│              │   │  (example)         │
│ • Entity     │   │  • Domain          │
│ • IModule    │   │  • Application     │
│ • BaseDbCtx  │   │  • Infrastructure  │
│ • IEventBus  │   │  • Endpoints       │
│ • BaseWorker │   │  • ProdutosModule  │
│ • Result<T>  │   └────────────────────┘
│ • IRequest   │
└──────────────┘
```
