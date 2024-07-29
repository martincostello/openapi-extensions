// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/hello", (
    [Description("The name of the person to greet.")]
    [OpenApiExample("Martin")]
    string? name) =>
    {
        if (string.IsNullOrEmpty(name))
        {
            return Results.Problem("No name was provided.", statusCode: StatusCodes.Status400BadRequest);
        }

        var greeting = new Greeting()
        {
            Text = $"Hello, {name}!",
        };

        return Results.Json(greeting);
    })
    .Produces<Greeting>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesOpenApiResponse(StatusCodes.Status200OK, "A greeting.")
    .ProducesOpenApiResponse(StatusCodes.Status400BadRequest, "No name was provided.");

app.Run();

namespace MartinCostello.OpenApi
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
