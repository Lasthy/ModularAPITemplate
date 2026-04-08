---
name: "Create Module"
description: "Use when you want to scaffold a new module, wire it into Host, and validate integration with a build checklist."
argument-hint: "ModuleName=<Name> SolutionPrefix=<Prefix optional> Depth=<quick|deep>"
agent: "Module Scaffolder"
---
Create a new module in this repository and complete registration end-to-end.

User input:
{{input}}

Required behavior:
1. Parse inputs:
- ModuleName is required.
- SolutionPrefix is optional and should default to the current solution name (derived from the existing .slnx file name).
- If solution name cannot be resolved, fallback to ModularAPITemplate.
- Depth controls explanation detail: quick by default, deep only if explicitly requested.

2. Discover and validate repository context before changes:
- Read [README.md](../../README.md) and [architecture.md](../../architecture.md).
- Confirm expected paths and naming conventions for this template.
- Check whether the module already exists under src/Modules and tests/Modules.

3. Scaffold the module using template conventions:
- Use dotnet new modularapi-module with ModuleName and SolutionPrefix as the default path.
- Do not hand-create module files when the template command is available.
- Only fall back to manual scaffolding if CLI/template execution is unavailable, and explicitly report that fallback.

4. Wire the module into the solution and host:
- Add module and test projects to solution folders.
- Add module project reference to Host project.
- Register module and endpoint mapping in [src/Host/Program.cs](../../src/Host/Program.cs) when needed by current registration style.
- Add or update module configuration in appsettings files.
- Required: add a `Modules:<ModuleName>` section to [src/Host/appsettings.Development.json](../../src/Host/appsettings.Development.json) using the same default shape as `Modules:Example`, including:
	- `ConnectionString`
	- `Inbox` defaults
	- `Outbox` defaults
- If configuration is also needed in [src/Host/appsettings.json](../../src/Host/appsettings.json), add a safe non-secret baseline and keep secrets/environment-specific values in development or environment variables.

5. Provide a strict implementation checklist in output:
Task Input Checklist
- [ ] Parsed ModuleName, SolutionPrefix, and Depth.

Scaffolding Checklist
- [ ] Module project created.
- [ ] Test project created.
- [ ] Folder and namespace conventions match repository patterns.

Wiring Checklist
- [ ] Projects added to solution folders.
- [ ] Host project references module project.
- [ ] Host registration and endpoint mapping aligned with existing architecture.
- [ ] App settings updated with `Modules:<ModuleName>` default configuration.
- [ ] `ConnectionString`, `Inbox`, and `Outbox` defaults present for the new module.

Validation Checklist
- [ ] Build command executed from repository root.
- [ ] Build result summarized with any errors/warnings.
- [ ] Any follow-up manual steps listed.

6. Validation command:
- Run dotnet build against the current solution file (*.slnx) from repository root.

7. Guardrails:
- Do not overwrite unrelated user changes.
- Preserve existing architecture style over introducing alternatives.
- If blocked, report exactly what is missing and the minimal manual command(s) to proceed.
