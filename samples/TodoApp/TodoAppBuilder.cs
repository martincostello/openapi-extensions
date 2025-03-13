// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace TodoApp;

/// <summary>
/// Extension methods for configuring the TodoApp application.
/// </summary>
public static class TodoAppBuilder
{
    /// <summary>
    /// Adds TodoApp services to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>
    /// The value passed by <paramref name="builder"/> for chaining.
    /// </returns>
    public static WebApplicationBuilder AddTodoApp(this WebApplicationBuilder builder)
    {
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

            // Add custom example providers for GUIDs, DateTimes and ProblemDetails
            options.AddExample<Guid, GuidExampleProvider>();
            options.AddExample<DateTime, DateTimeExampleProvider>();
            options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();

            // Configure XML comments for the schemas in the OpenAPI document
            options.AddXmlComments<Program>();

            // Add a custom transformation for the descriptions in the OpenAPI document
            options.DescriptionTransformations.Add((p) => p.Replace(" This class cannot be inherited.", string.Empty, StringComparison.Ordinal));
        });

        if (!builder.Environment.IsDevelopment())
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

        if (string.Equals(builder.Configuration["CODESPACES"], "true", StringComparison.OrdinalIgnoreCase))
        {
            // When running in GitHub Codespaces, X-Forwarded-Host also needs to be set
            builder.Services.Configure<ForwardedHeadersOptions>(
                options => options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost);
        }

        return builder;
    }

    /// <summary>
    /// Configures the specified <see cref="WebApplication"/> to use TodoApp.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>
    /// The value passed by <paramref name="app"/> for chaining.
    /// </returns>
    public static WebApplication UseTodoApp(this WebApplication app)
    {
        // Configure error handling
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
        app.MapOpenApiYaml();

        // Add the HTTP endpoints
        app.MapTodoApiRoutes();

        return app;
    }
}
