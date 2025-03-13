// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using MartinCostello.OpenApi;
using MartinCostello.WebApi;
using MartinCostello.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddControllers().AddJsonOptions((p) => ConfigureJsonSerialization(p.JsonSerializerOptions));
builder.Services.ConfigureHttpJsonOptions((p) => ConfigureJsonSerialization(p.SerializerOptions));

builder.Services.AddOpenApi();
builder.Services.AddOpenApiExtensions((p) => p.AddXmlComments<TimeModel>());

var app = builder.Build();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Time}/{action=Now}/{id?}");

app.MapOpenApi();

app.Run();

static void ConfigureJsonSerialization(JsonSerializerOptions options)
    => options.TypeInfoResolverChain.Add(MvcSerializerContext.Default);
