# OpenAPI Extensions for ASP.NET Core

[![NuGet][package-badge-version]][package-download]
[![NuGet Downloads][package-badge-downloads]][package-download]

[![Build status][build-badge]][build-status]
[![codecov][coverage-badge]][coverage-report]
[![OpenSSF Scorecard][scorecard-badge]][scorecard-report]

## Introduction

A NuGet package of extensions for the [Microsoft.AspNetCore.OpenApi][aspnetcore-openapi] package.

Features include:

- Adding examples to OpenAPI operations and schemas.
- Customizing descriptions for:
  - OpenAPI operation parameters and responses;
  - OpenAPI schemas and their properties.
- Adding application URLs to the OpenAPI document.
- Adding OpenAPI schema documentation from XML comments.
- Adding an HTTP endpoint to get OpenAPI documents as YAML.

The library is also designed to be compatible with support for [native AoT][aspnetcore-native-aot] in ASP.NET Core 9.

There is also [a sample application using the library][sample-app].

An overview of the library and how it works can be found on YouTube in this talk from
.NET Conf 2024: [📺 _Extending ASP.NET Core OpenAPI_][dotnet-conf]

## Installation

To install the library from [NuGet][package-download] using the .NET SDK run the following command:

```console
dotnet add package MartinCostello.OpenApi.Extensions
```

## Usage

Below is an example code snippet showing how to use the features of the library:

```csharp
using System.ComponentModel;
using System.Text.Json.Serialization;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi((options) =>
{
    // Configure ASP.NET Core support for OpenAPI documentation...
});

builder.Services.AddOpenApiExtensions((options) =>
{
    // Always return the server URLs in the OpenAPI document
    // Only enable this option in production if you are sure
    // you wish to explicitly expose your server URLs.
    options.AddServerUrls = true;

    // Set a default URL to use for generation of the OpenAPI document using
    // https://www.nuget.org/packages/Microsoft.Extensions.ApiDescription.Server.
    options.DefaultServerUrl = "https://localhost:50001";

    // Add examples for OpenAPI operations and components
    options.AddExamples = true;

    // Add JSON serialization context to use to serialize examples when enabled
    options.SerializationContexts.Add(TodoJsonSerializerContext.Default);

    // Add a custom example provider for ProblemDetails
    options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();

    // Configure XML comments for the schemas in the OpenAPI document
    // from the assembly that the Program class is defined in.
    options.AddXmlComments<Program>();

    // Add a custom transformation for the descriptions in the OpenAPI document
    options.DescriptionTransformations.Add((p) => p.ToUpper());
});

// Required if AddServerUrls=true
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure endpoint to get OpenAPI documents as JSON
app.MapOpenApi();

// Optionally also (or instead) configure endpoint to get OpenAPI documents as YAML
app.MapOpenApiYaml();

// The [Description] attribute can be used to add parameter descriptions
// The [OpenApiExample] attribute can be used to add simple string examples
// The ProducesOpenApiResponse() method can be used to customize the description for responses
app.MapGet("/todo", ([Description("The Todo item's ID"), OpenApiExample("42")] string id) =>
{
    return new Todo()
    {
        Id = id,
        Text = "Example",
        IsComplete = false,
    };
}).ProducesOpenApiResponse(StatusCodes.Status200OK, "The Todo item.");

app.MapPost("/todo", (Todo model) =>
{
    var todo = new Todo()
    {
        Id = Guid.NewGuid().ToString(),
        Text = model.Text,
        IsComplete = false,
    };
    return Results.Created($"/todo/{todo.Id}", todo);
}).ProducesOpenApiResponse(StatusCodes.Status201Created, "The created Todo item.");

app.Run();

// Classes can implement IExampleProvider<Todo> and decorate
// themselves with the [OpenApiExample<T>] attribute to use
// the example for all usage of the type in the OpenAPI document.
// Examples can also be added as attributes to parameters of operations,
// endpoint methods themselves, or as endpoint metadata.

[OpenApiExample<Todo>]
public class Todo : IExampleProvider<Todo>
{
    public string Id { get; set; }
    public string Text { get; set; }
    public bool IsComplete { get; set; }

    public static Todo GenerateExample() =>
        new()
        {
            Id = "42",
            Text = "Buy milk",
            IsComplete = false,
        };
}

// Custom IExampleProvider<T> implementations can be used to add more specific
// examples for types in the OpenAPI document, or for types that are not owned
// by the application itself (e.g. ASP.NET Core's ProblemDetails class).

public class ProblemDetailsExampleProvider : IExampleProvider<ProblemDetails>
{
    public static ProblemDetails GenerateExample()
    {
        return new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "The specified value is invalid.",
        };
    }
}

// JSON source generation context to use to generate examples for JSON
// payloads that matches the runtime behaviour of the application itself
// (for example whether to use camelCase or PascalCase etc.)

[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
public partial class TodoJsonSerializerContext : JsonSerializerContext;
```

## Building and Testing

Compiling the application yourself requires Git and the [.NET SDK][dotnet-sdk] to be installed.

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/openapi-extensions.git
cd openapi-extensions
./build.ps1
```

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/openapi-extensions.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[aspnetcore-native-aot]: https://learn.microsoft.com/aspnet/core/fundamentals/native-aot "ASP.NET Core support for Native AOT"
[aspnetcore-openapi]: https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi
[build-badge]: https://github.com/martincostello/openapi-extensions/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/openapi-extensions/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush "Continuous Integration for this project"
[coverage-badge]: https://codecov.io/gh/martincostello/openapi-extensions/branch/main/graph/badge.svg
[coverage-report]: https://codecov.io/gh/martincostello/openapi-extensions "Code coverage report for this project"
[dotnet-conf]: https://www.youtube.com/watch?v=ooP0vkST3X8 "Extending ASP.NET Core OpenAPI - .NET Conf 2024"
[dotnet-sdk]: https://dotnet.microsoft.com/download "Download the .NET SDK"
[issues]: https://github.com/martincostello/openapi-extensions/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
[package-badge-downloads]: https://img.shields.io/nuget/dt/MartinCostello.OpenApi.Extensions?logo=nuget&label=Downloads&color=blue
[package-badge-version]: https://img.shields.io/nuget/v/MartinCostello.OpenApi.Extensions?logo=nuget&label=Latest&color=blue
[package-download]: https://www.nuget.org/packages/MartinCostello.OpenApi.Extensions "Download MartinCostello.OpenApi.Extensions from NuGet"
[repo]: https://github.com/martincostello/openapi-extensions "This project on GitHub.com"
[sample-app]: https://github.com/martincostello/openapi-extensions/tree/main/samples/TodoApp "Sample application using the library"
[scorecard-badge]: https://api.securityscorecards.dev/projects/github.com/martincostello/openapi-extensions/badge
[scorecard-report]: https://securityscorecards.dev/viewer/?uri=github.com/martincostello/openapi-extensions "OpenSSF Scorecard for this project"
