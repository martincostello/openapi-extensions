// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using TodoApp;

// Create the default web application builder
var builder = WebApplication.CreateBuilder(args);

// Configure the Todo repository and associated services
builder.Services.AddTodoApi();

// Configure OpenAPI documentation for the Todo API
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Todo API";
        document.Info.Description = "An API for managing Todo items.";
        document.Info.Version = "v1";

        document.Info.Contact = new()
        {
            Name = "martincostello",
            Url = new("https://github.com/martincostello/openapi-extensions"),
        };

        document.Info.License = new()
        {
            Name = "Apache 2.0",
            Url = new("https://www.apache.org/licenses/LICENSE-2.0"),
        };

        return Task.CompletedTask;
    });
});

// Configure extensions for OpenAPI
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApiExtensions(options =>
{
    // Always return the server URLs in the OpenAPI document
    options.AddServerUrls = true;

    // Set a default URL to use for generation of the OpenAPI document using
    // https://www.nuget.org/packages/Microsoft.Extensions.ApiDescription.Server.
    options.DefaultServerUrl = "https://localhost:50001";

    // Add examples for OpenAPI operations and components
    options.AddExamples = true;

    // Add JSON serialization context to use to serialize examples
    options.SerializationContexts.Add(TodoJsonSerializerContext.Default);

    // Add custom example providers for GUIDs and ProblemDetails
    options.AddExample<Guid, GuidExampleProvider>();
    options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();

    // Configure XML comments for the schemas in the OpenAPI document
    options.AddXmlComments<Program>();

    // Add a custom transformation for the descriptions in the OpenAPI document
    options.DescriptionTransformations.Add((p) => p.Replace(" This class cannot be inherited.", string.Empty, StringComparison.Ordinal));
});

if (string.Equals(builder.Configuration["CODESPACES"], "true", StringComparison.OrdinalIgnoreCase))
{
    // When running in GitHub Codespaces, X-Forwarded-Host also needs to be set
    builder.Services.Configure<ForwardedHeadersOptions>(
        options => options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost);
}

// Create the app
var app = builder.Build();

// Configure error handling
if (!app.Environment.IsDevelopment())
{
    builder.Services.AddProblemDetails();
    builder.Services.Configure<ProblemDetailsOptions>((options) =>
    {
        options.CustomizeProblemDetails = (context) =>
        {
            if (context.Exception is not null)
            {
                context.ProblemDetails.Detail = "An internal error occurred.";
            }

            context.ProblemDetails.Instance = context.HttpContext.Request.Path;
            context.ProblemDetails.Title = ReasonPhrases.GetReasonPhrase(context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError);
        };
    });
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

// Require use of HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Add static files for SwaggerUI
app.UseStaticFiles();

// Add endpoints for OpenAPI
app.MapOpenApi();

// Add the HTTP endpoints
app.MapTodoApiRoutes();

// Run the application
app.Run();

namespace TodoApp
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
