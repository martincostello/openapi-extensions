// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using MartinCostello.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/hello", (
    [Description("The name of the person to greet.")]
    [OpenApiExample("Martin")]
    string name) =>
{
    var greeting = new Greeting()
    {
        Text = $"Hello, {name}!",
    };

    return greeting;
})
    .WithMetadata(new OpenApiResponseAttribute(StatusCodes.Status200OK, "A greeting."));

app.Run();

namespace MartinCostello.OpenApi
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
