# Coding Agent Instructions

This file provides guidance to coding agents when working with code in this repository.

## Build, test, and lint commands

- Canonical local validation is `pwsh ./build.ps1`. The script bootstraps the SDK version from `global.json`, packs `src/OpenApi.Extensions\MartinCostello.OpenApi.Extensions.csproj`, and then runs the test suite in `Release`.
- Use `pwsh ./build.ps1 -SkipTests` when you only need packaging/build output.
- Run the main test project directly with `dotnet test tests\OpenApi.Extensions.Tests\MartinCostello.OpenApi.Extensions.Tests.csproj --configuration Release`.
- Run one target framework explicitly with `dotnet test tests\OpenApi.Extensions.Tests\MartinCostello.OpenApi.Extensions.Tests.csproj --configuration Release --framework net10.0`.
- Run a single test with a filter such as `dotnet test tests\OpenApi.Extensions.Tests\MartinCostello.OpenApi.Extensions.Tests.csproj --configuration Release --framework net10.0 --filter "FullyQualifiedName~MartinCostello.OpenApi.AssemblyTests.Library_Is_Strong_Named" /p:CollectCoverage=false`. The test project enforces an 85% coverage threshold, so disable coverage for narrowly filtered runs.
- CI linting is defined in `.github/workflows/lint.yml`. The repo currently lint-checks Markdown, GitHub Actions workflows, and PowerShell scripts. The PowerShell script lint pass uses `Invoke-ScriptAnalyzer -Path . -Recurse -ReportSummary -Settings @{ IncludeDefaultRules = $true; Severity = @('Error', 'Warning') }`.

## High-level architecture

- This repository is centered on a single package project: `src\OpenApi.Extensions\MartinCostello.OpenApi.Extensions.csproj`. It extends `Microsoft.AspNetCore.OpenApi` rather than replacing it.
- The main composition point is `OpenApiExtensions.AddOpenApiExtensions(...)` in `src\OpenApi.Extensions\OpenApiExtensions.cs`. That method wires repository features into ASP.NET Core OpenAPI by configuring `OpenApiOptions` and registering keyed transformers per document name.
- Feature behavior is implemented through transformer classes in `src\OpenApi.Extensions\Transformers\`. `AddExamplesTransformer`, `AddParameterDescriptionsTransformer`, `AddResponseDescriptionsTransformer`, `AddSchemaXmlDocumentationTransformer`, `AddOperationXmlDocumentationTransformer`, `AddServersTransformer`, and `DescriptionsTransformer` form the actual pipeline that mutates operations, schemas, and documents.
- `OpenApiExtensionsOptions` is the central configuration object. Examples, XML comment sources, description transformations, serializer contexts, and server URL behavior are all modeled there, with fluent helpers such as `AddExample<TSchema>()` and `AddXmlComments<T>()`.
- The library is multi-targeted for `net9.0` and `net10.0`. When framework behavior differs, this repo usually uses target-specific companion files such as `ExampleFormatter.net9.0.cs` and `ExampleFormatter.net10.0.cs` instead of pushing all differences into one file.
- `OpenApiEndpointRouteBuilderExtensions.cs` and `OpenApiEndpointConventionBuilderExtensions.cs` provide the public endpoint-facing helpers, including YAML document mapping and response-description metadata for minimal APIs.
- Tests are intentionally broader than pure unit tests. `tests\OpenApi.Extensions.Tests` uses xUnit v3 plus Verify snapshots to validate generated OpenAPI output, while `tests\TestApp`, `tests\WebApi`, `tests\Models.A`, and `tests\Models.B` act as companion projects that exercise the package against realistic apps and cross-assembly model scenarios.
- `samples\TodoApp` is the reference app for package usage and is also referenced by the test project, so changes there can affect both documentation/examples and integration coverage.

## Key conventions

- Treat `pwsh ./build.ps1` as the authoritative local workflow. `.github\CONTRIBUTING.md` expects changes to build cleanly with no compiler warnings and all tests passing.
- If you change the public API surface, check `src\OpenApi.Extensions\PublicAPI\` and its target-framework subfolders. This repo tracks API compatibility with PublicApiAnalyzers and keeps shipped/unshipped API files under source control.
- When adding OpenAPI behavior, prefer integrating it through the existing transformer pipeline and `OpenApiExtensionsOptions` instead of creating parallel registration paths.
- Example generation is source-generation-aware. If `AddExamples` is enabled, the code expects one or more `JsonSerializerContext` instances in `OpenApiExtensionsOptions.SerializationContexts`; tests and samples follow that pattern.
- Snapshot tests live beside the test files as `*.verified.txt` files and commonly use `UniqueForTargetFrameworkAndVersion()`, so output assertions are usually split per framework. Expect to update or add verified files when OpenAPI output changes.
- The test suite uses `WebApplicationFactory` fixtures (`MinimalFixture`, `MvcFixture`) to build small apps around the package. Reuse those fixtures and companion test projects instead of inventing ad hoc hosts.
- Keep multi-targeting explicit. This codebase already uses conditional files and `#if NET9_0` / `#if NET10_0_OR_GREATER` where ASP.NET Core OpenAPI APIs diverge.
- Follow the repository formatting and documentation rules in `.editorconfig` and `stylecop.json`: C# files use UTF-8 with BOM, CRLF, file-scoped namespaces, and the standard copyright header, and public APIs are expected to have XML documentation.

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not change the public API unless specifically requested.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
