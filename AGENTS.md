# Repository Guidelines

## Project Structure & Module Organization
The solution is `TalentManagementAPI.slnx` with clean-layered projects under the repo root:
- `TalentManagementAPI.Domain`: core entities and domain rules.
- `TalentManagementAPI.Application`: use cases, contracts, and business orchestration.
- `TalentManagementAPI.Infrastructure.Persistence` and `TalentManagementAPI.Infrastructure.Shared`: data access and cross-cutting infrastructure.
- `TalentManagementAPI.WebApi`: HTTP API host and composition root.
- `*.Tests` projects mirror runtime layers (`Application.Tests`, `Infrastructure.Tests`, `WebApi.Tests`).
- `docs/` contains supporting documentation; `docker-compose*.yml` defines local container setup.

## Build, Test, and Development Commands
Run commands from repository root:
- `dotnet restore TalentManagementAPI.slnx` - restore NuGet packages.
- `dotnet build TalentManagementAPI.slnx -c Debug` - compile all projects.
- `dotnet test TalentManagementAPI.slnx --collect:"XPlat Code Coverage"` - run full test suite with coverage collection.
- `dotnet run --project TalentManagementAPI.WebApi` - start the API locally.
- `docker compose up -d` - start containerized dependencies and API stack.

## Coding Style & Naming Conventions
Use standard C# conventions and keep code consistent with existing files:
- 4-space indentation; braces on new lines; one public type per file.
- `PascalCase` for types/methods/properties, `camelCase` for locals/parameters, `_camelCase` for private fields.
- Keep namespaces aligned to folder and project names (for example `TalentManagementAPI.Application.*`).
- Prefer small, focused classes and constructor injection for dependencies.

## Testing Guidelines
Testing uses `xUnit`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`, and `coverlet.collector`.
- Name test files by subject (for example `EmployeeServiceTests.cs`).
- Use `MethodName_StateUnderTest_ExpectedBehavior` style for test methods.
- Add/adjust tests in the matching `*.Tests` project for every behavior change.
- Run `dotnet test` before opening a pull request.

## Commit & Pull Request Guidelines
Git history is not readable in this environment due safe-directory restrictions, so follow this repository convention:
- Commit messages: imperative, concise, and scoped (for example `WebApi: validate department id on create`).
- Keep commits focused; avoid mixing refactors with behavior changes.
- PRs should include: purpose, affected projects, test evidence (`dotnet test` output), and API contract changes (request/response examples) when relevant.
- Link related issues/work items and call out any required config or migration steps.

## Security & Configuration Tips
- Never commit secrets; use environment variables or local user-secrets for sensitive settings.
- Review `docker-compose.override.yml` and app settings before sharing environments.
- Validate connection strings and external endpoints per environment (local, CI, production).
