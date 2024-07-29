// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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

var app = builder.Build();

app.MapOpenApi();

app.Run();

namespace MartinCostello.OpenApi
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
