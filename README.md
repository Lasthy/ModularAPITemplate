# ModularAPITemplate

A .NET 10 template for building **modular Web APIs** with Clean Architecture. Each module is self-contained with its own domain, persistence, and endpoints — tied together by a shared kernel and a lightweight host.

## Features

- **Modular architecture** — each domain is an isolated module with its own DbContext, endpoints, and DI
- **Clean Architecture** per module — Domain, Application, Infrastructure, Endpoints layers
- **MediatR** — use cases as Commands/Queries with handlers
- **Integration events** — cross-module communication via `IEventBus` (in-process or external)
- **Domain events** — automatically dispatched on `SaveChangesAsync`
- **Request context** — `IRequestContext` provides authenticated user info to handlers and services, extensible per module
- **Background workers** — `BaseWorker` with scoped DI, logging, and error handling
- **OpenAPI docs** — each module gets its own document, with Scalar or Swagger UI
- **Result pattern** — `Result<T>` for use case responses without exceptions
- **Module template** — scaffold new modules with a single CLI command

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Installation

### Install the templates

From the root of this repository:

```bash
dotnet new install .
```

This installs two templates:

| Template | Short Name | Description |
|---|---|---|
| Modular API Template | `modularapi` | Full solution with Host, SharedKernel, and example modules |
| Modular API - Module | `modularapi-module` | New module (src + tests) for an existing solution |

### Verify installation

```bash
dotnet new list modularapi
```

## Usage

### 1. Create a new solution

```bash
dotnet new modularapi -n MyProject -o MyProject
cd MyProject
```

This generates:

```
MyProject/
├── src/
│   ├── Host/MyProject.Host/           → Entry point (Program.cs)
│   ├── SharedKernel/MyProject.SharedKernel/  → Shared abstractions
│   └── Modules/
│       ├── Produtos/                   → Example module
│       └── Pedidos/                    → Example module
├── tests/
│   └── Modules/
│       ├── Produtos/
│       └── Pedidos/
└── MyProject.slnx
```

### 2. Build and run

```bash
dotnet build
dotnet run --project src/Host/MyProject.Host
```

The API docs will be available at `/scalar` (default) or `/swagger` (configurable in `appsettings.json`).

### 3. Add a new module

From the solution root:

```bash
dotnet new modularapi-module -n Clientes --SolutionPrefix MyProject
```

Then complete the setup:

1. **Add projects to the solution:**
   ```bash
   dotnet sln add src/Modules/Clientes/MyProject.Modules.Clientes.csproj
   dotnet sln add tests/Modules/Clientes/MyProject.Modules.Clientes.Tests.csproj
   ```

2. **Add module reference to Host:**
   ```bash
   dotnet add src/Host/MyProject.Host/MyProject.Host.csproj reference src/Modules/Clientes/MyProject.Modules.Clientes.csproj
   ```

3. **Register in `Program.cs`:**
   ```csharp
   builder.Services.AddModule<ClientesModule>(builder.Configuration);
   app.MapModuleEndpoints<ClientesModule>();
   ```

4. **Add connection string to `appsettings.json`:**
   ```json
   "ConnectionStrings": {
     "ClientesDb": "Server=localhost;Database=MyProject;Trusted_Connection=true;"
   }
   ```

## Configuration

### OpenAPI UI

In `appsettings.json`:

```json
{
  "OpenApi": {
    "UI": "Scalar"
  }
}
```

Supported values: `"Scalar"` (default) or `"Swagger"`.

### Connection Strings

Each module has its own connection string. Use environment variables in production and `appsettings.Development.json` for local development.

## Architecture

See [.ARCHITECTURE.md](.ARCHITECTURE.md) for the full architecture reference (English, source of truth).

A Portuguese version is also available: [.ARCHITECTURE.pt-BR.md](.ARCHITECTURE.pt-BR.md).

## AI-Assisted Development

This template includes a Copilot configuration at `.copilot/.description.txt` with project-specific instructions. **When starting a new project from this template, update the `<project_details>` section** in that file to describe your specific project's domain and requirements.

## Uninstalling the templates

```bash
dotnet new uninstall <path-to-this-repo>
```

## License

MIT
