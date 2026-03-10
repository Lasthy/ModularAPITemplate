# Arquitetura — ModularAPITemplate

> **Versão em português.** A versão em inglês ([.ARCHITECTURE.md](.ARCHITECTURE.md)) é a fonte da verdade.

## Visão Geral

ModularAPITemplate é um template para desenvolvimento de Web APIs modulares em .NET 10. Cada domínio/módulo é autocontido e responsável por suas próprias regras de negócio, persistência e endpoints, enquanto a infraestrutura compartilhada é centralizada no **SharedKernel**.

## Estrutura de Projetos

```
src/
├── Host/                                  → Projeto Web (entry point)
├── SharedKernel/                          → Abstrações e infra compartilhada
└── Modules/
    └── <NomeModulo>/  → Módulo de domínio (csproj na raiz da pasta)

tests/
└── Modules/
    └── <NomeModulo>/  → Testes do módulo (csproj na raiz da pasta)
```

## Camadas por Módulo

Cada módulo segue Clean Architecture internamente:

```
ModularAPITemplate.Modules.<NomeModulo>/
├── Domain/
│   ├── Entities/       → Entidades e agregados
│   └── Events/         → Eventos de domínio
├── Application/
│   ├── DTOs/           → Data Transfer Objects e mapeamentos
│   └── UseCases/       → Casos de uso (Commands/Queries + Handlers via MediatR)
├── Infrastructure/
│   └── Persistence/    → DbContext e configurações EF
├── Endpoints/          → Minimal API endpoints
└── <NomeModulo>Module.cs  → Registro de DI e endpoints (implementa IModule)
```

## Regras de Arquitetura

### 1. Isolamento de Módulos
- **Cada módulo é autocontido**: possui seus próprios DbContext, endpoints e registros de DI.
- **Módulos NÃO devem referenciar outros módulos diretamente**. Comunicação entre módulos deve ser feita via **eventos de integração** (`IEventBus`).
- Cada módulo define seu próprio schema no banco de dados.

### 2. Dependências Permitidas
```
Host → SharedKernel, Módulos
Módulo → SharedKernel (somente)
SharedKernel → Pacotes NuGet (sem dependência de módulos)
```

### 3. Registro de Módulos
- Todo módulo deve implementar `IModule` (definido no SharedKernel), incluindo a propriedade `ModuleName`.
- `ModuleName` é usado como identificador do documento OpenAPI do módulo.
- O **Host** registra cada módulo com:
  ```csharp
  builder.Services.AddModule<MeuModulo>(builder.Configuration);
  app.MapModuleEndpoints<MeuModulo>();
  ```
- `AddModule` registra automaticamente um documento OpenAPI exclusivo para o módulo.
- Isso garante que o Host não precisa conhecer a implementação interna do módulo.

### 4. Persistência
- Cada módulo possui seu próprio `DbContext` herdando de `BaseDbContext`.
- `BaseDbContext` despacha eventos de domínio automaticamente no `SaveChangesAsync`.
- Cada módulo deve definir seu próprio **schema** no banco para evitar colisões.
- Use cases utilizam o `DbContext` do módulo diretamente para persistência.
- **NÃO** execute migrations automaticamente. Use a CLI do EF Core: `dotnet ef migrations add <Nome> -p <Projeto> -s <Host>`.

### 5. Use Cases (Casos de Uso)
- Use cases são implementados como **Commands** (escrita) ou **Queries** (leitura) via MediatR.
- Cada use case fica em seu próprio diretório com Command/Query + Handler.
- Retornam `Result<T>` para encapsular sucesso/falha sem exceções.

### 6. Domain Events (Eventos de Domínio)
- Entidades levantam eventos de domínio via `RaiseDomainEvent()`.
- Eventos são despachados automaticamente pelo `BaseDbContext` antes de salvar.
- Use `DomainEvent` (record base) para criar eventos tipados.

### 7. Integration Events (Eventos de Integração)
- Use `IEventBus.PublishAsync()` para comunicação entre módulos.
- A implementação padrão (`InProcessEventBus`) usa MediatR in-process.
- Pode ser substituída por RabbitMQ, Kafka, Azure Service Bus etc.

### 8. Comunicação Entre Módulos (Cross-Module)
- **Módulos NUNCA referenciam outros módulos diretamente.** Toda comunicação é feita via eventos de integração (`IEventBus`).
- O módulo de origem levanta um **domain event** na entidade, que é despachado pelo `BaseDbContext`.
- Um handler de domain event dentro do mesmo módulo publica um **integration event** via `IEventBus.PublishAsync()`.
- O módulo de destino implementa um handler (`INotificationHandler<T>`) para o integration event e executa sua lógica.
- **Contratos compartilhados** (integration events, constantes de roles/policies) ficam no **SharedKernel**.
- Cada módulo cria e mantém suas **próprias entidades**, mesmo que representem o mesmo conceito real. Ex: `Usuario` (Identity) → `Cliente` (Loja), `Vendedor` (Vendas).
- A consistência entre módulos é **eventual** — falhas em módulos consumidores não desfazem operações do módulo de origem.

#### Exemplo de fluxo:
```
Identity: CriarUsuario → UsuarioCriado (domain event)
  → handler publica UsuarioCriadoIntegrationEvent { UsuarioId, Perfis[] }

Loja: handler recebe evento → if Perfis.Contains("Cliente") → cria Cliente
Vendas: handler recebe evento → if Perfis.Contains("Vendedor") → cria Vendedor
```

#### Autorização cross-module:
- **Identity** é o dono de autenticação, roles e JWT.
- Constantes de roles e nomes de policies ficam no **SharedKernel** (ex: `Perfis.Cliente`, `AuthPolicies.ApenasVendedor`).
- **Identity** registra as policies (`AddAuthorizationBuilder().AddPolicy(...)`).
- **Qualquer módulo** aplica policies nos seus endpoints via `.RequireAuthorization(AuthPolicies.ApenasCliente)`.

### 9. Workers
- Workers herdam de `BaseWorker` (que herda de `BackgroundService`).
- Recebem `IServiceScopeFactory` para resolver dependências scoped.
- Incluem logging e tratamento de erro padronizado.

### 10. Endpoints
- Use **Minimal APIs** com `MapGroup()` para agrupar endpoints por módulo.
- Use `.WithGroupName(ModuleName)` para vincular o grupo ao documento OpenAPI do módulo.
- Documente todos os endpoints com `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`.
- Use `.WithTags()` para organização no OpenAPI.

### 11. Documentação da API (OpenAPI)
- Cada módulo possui seu próprio documento OpenAPI, registrado automaticamente por `AddModule`.
- A UI de documentação é configurável via `appsettings.json`:
  ```json
  "OpenApi": { "UI": "Scalar" }
  ```
- Valores suportados: `"Scalar"` (padrão) ou `"Swagger"`.
- Em desenvolvimento, a UI fica disponível em `/scalar` (Scalar) ou `/swagger` (Swagger).

### 12. Contexto da Requisição (Request Context)
- `IRequestContext` (SharedKernel) fornece informações do usuário autenticado: `UserId`, `UserName`, `IsAuthenticated`, `Roles`, `Claims`.
- `RequestContext` implementa `IRequestContext` lendo dados do `HttpContext.User` (via `IHttpContextAccessor`).
- É registrado automaticamente como **Scoped** pelo `AddModule` — não precisa de registro manual.
- **Pode ser injetado em handlers, use cases e serviços** para acessar informações do usuário logado.
- **Módulos podem criar contextos estendidos** com informações específicas do domínio:
  ```csharp
  // No módulo Loja:
  public interface ILojaRequestContext : IRequestContext
  {
      Guid? ClienteId { get; }
      bool IsClienteAtivo { get; }
  }

  public class LojaRequestContext(IRequestContext inner, LojaDbContext db)
      : ILojaRequestContext
  {
      // Delega propriedades base para inner
      // Resolve ClienteId consultando o banco pelo UserId
  }
  ```
- O contexto estendido é registrado no `RegisterServices` do módulo:
  ```csharp
  services.AddScoped<ILojaRequestContext, LojaRequestContext>();
  ```

### 13. Nomenclatura
- Entidades, casos de uso e conceitos de domínio devem ser nomeados em **pt-BR** (português brasileiro).
- Nomes de infraestrutura/framework podem permanecer em inglês.
- **Logging deve ser sempre em inglês**, para consistência e facilidade de pesquisa em ferramentas de monitoramento.

### 14. Testes
- Cada módulo deve ter seu projeto de testes correspondente.
- Testes de domínio validam regras de negócio (entidades, value objects).
- Testes de application validam use cases com mocks (NSubstitute).
- Estrutura espelha o módulo: `Tests/Domain/`, `Tests/Application/UseCases/`.

### 15. Configuração
- Connection strings e configurações sensíveis devem usar **variáveis de ambiente** em produção.
- Em desenvolvimento, use `appsettings.Development.json`.
- Nunca commite secrets no controle de versão.

## Como Criar um Novo Módulo

### Usando o template (recomendado):
```bash
# A partir da raiz da solution:
dotnet new modularapi-module -n <NomeModulo> --SolutionPrefix <PrefixoDaSolution>
```
Isso cria automaticamente a estrutura completa (src + testes) com todos os arquivos necessários.

### Passos pós-criação:
1. Adicione os projetos criados à solution (`.slnx`) dentro das pastas virtuais:
   ```bash
   dotnet sln add src/Modules/<NomeModulo>/<Prefixo>.Modules.<NomeModulo>.csproj --solution-folder Modules
   dotnet sln add tests/Modules/<NomeModulo>/<Prefixo>.Modules.<NomeModulo>.Tests.csproj --solution-folder Tests
   ```
2. Adicione a referência do módulo no `Host.csproj`.
3. Registre no `Program.cs` do Host:
   ```csharp
   builder.Services.AddModule<NomeModuloModule>(builder.Configuration);
   app.MapModuleEndpoints<NomeModuloModule>();
   ```
4. Adicione a connection string no `appsettings.json`:
   ```json
   "ConnectionStrings": { "NomeModuloDb": "sua-connection-string" }
   ```

### Estrutura gerada:
```
src/Modules/<NomeModulo>/
├── Domain/Entities/
├── Domain/Events/
├── Application/DTOs/
├── Application/UseCases/
├── Infrastructure/Persistence/<NomeModulo>DbContext.cs
├── Endpoints/
└── <NomeModulo>Module.cs

tests/Modules/<NomeModulo>/
├── Application/UseCases/
└── Domain/
```

### Manual (sem template):
1. Crie o projeto: `dotnet new classlib -n <Prefixo>.Modules.<Nome>`
2. Adicione referência ao SharedKernel e o FrameworkReference do ASP.NET Core.
3. Crie a estrutura de pastas: `Domain/`, `Application/`, `Infrastructure/`, `Endpoints/`.
4. Implemente `IModule` no arquivo `<Nome>Module.cs`.
5. Registre no Host: `AddModule<NomeModule>()` e `MapModuleEndpoints<NomeModule>()`.
6. Crie o projeto de testes correspondente.
7. Adicione ambos os projetos à solution com pastas virtuais:
   ```bash
   dotnet sln add src/Modules/<Nome>/<Prefixo>.Modules.<Nome>.csproj --solution-folder Modules
   dotnet sln add tests/Modules/<Nome>/<Prefixo>.Modules.<Nome>.Tests.csproj --solution-folder Tests
   ```
8. Adicione a connection string no `appsettings.json`.

## Diagrama de Dependências

```
┌──────────────────────────────────────────────┐
│                    Host                      │
│  (Program.cs — registra módulos e pipeline)  │
└──────┬───────────────────┬───────────────────┘
       │                   │
       ▼                   ▼
┌──────────────┐   ┌───────────────────┐
│ SharedKernel │◄──│  Módulo Produtos   │
│              │   │  (exemplo)         │
│ • Entity     │   │  • Domain          │
│ • IModule    │   │  • Application     │
│ • BaseDbCtx  │   │  • Infrastructure  │
│ • IEventBus  │   │  • Endpoints       │
│ • BaseWorker │   │  • ProdutosModule  │
│ • Result<T>  │   └───────────────────┘
│ • MediatR    │
└──────────────┘
```
