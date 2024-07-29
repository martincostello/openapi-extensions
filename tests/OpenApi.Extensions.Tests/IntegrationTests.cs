﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Models.A;
using Models.B;

namespace MartinCostello.OpenApi;

public class IntegrationTests(ITestOutputHelper outputHelper) : DocumentTests(outputHelper)
{
    [Fact]
    public async Task Schema_Is_Correct_For_App()
    {
        // Act and Assert
        await VerifyOpenApiDocumentAsync(
            (services) =>
            {
                services.AddHttpContextAccessor();
                services.AddOpenApi();
                services.AddOpenApiExtensions((options) =>
                {
                    options.AddExamples = true;
                    options.AddServerUrls = true;
                    options.SerializationContext = AppJsonSerializationContext.Default;

                    options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();
                    options.AddXmlComments<Greeting>();
                });
            },
            (endpoints) =>
            {
                endpoints.MapGet("/hello", (
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
            });
    }

    [Fact]
    public async Task Schema_Is_Correct_For_Classes()
    {
        // Act and Assert
        await VerifyOpenApiDocumentAsync(
            (services) =>
            {
                services.AddOpenApi();
                services.AddOpenApiExtensions((options) =>
                {
                    options.AddExamples = true;
                    options.SerializationContext = AnimalsJsonSerializationContext.Default;

                    options.AddXmlComments<Animal>();
                });
            },
            (endpoints) =>
            {
                endpoints.MapPost("/adopt/cat", (Cat cat) =>
                {
                    return TypedResults.NoContent();
                });

                endpoints.MapPost("/adopt/dog", (Dog dog) =>
                {
                    return TypedResults.NoContent();
                });

                endpoints.MapGet("/animals", () =>
                {
                    return new Animal[]
                    {
                        new Cat() { Name = "Garfield", Color = "Orange" },
                        new Dog() { Name = "Snoopy", Breed = "Beagle" },
                    };
                });
            });
    }

    [Fact]
    public async Task Schema_Is_Correct_For_Records()
    {
        // Act and Assert
        await VerifyOpenApiDocumentAsync(
            (services) =>
            {
                services.AddOpenApi();
                services.AddOpenApiExtensions((options) =>
                {
                    options.AddExamples = true;
                    options.SerializationContext = VehiclesJsonSerializationContext.Default;

                    options.AddXmlComments<Vehicle>();
                });
            },
            (endpoints) =>
            {
                endpoints.MapPost("/car", (Car car) =>
                {
                    return TypedResults.NoContent();
                });

                endpoints.MapGet("/vehicles", () =>
                {
                    return new Vehicle[]
                    {
                        new Car(CarType.Coupe, 4, "Aston Martin"),
                        new Motorcycle("Yamaha"),
                    };
                });
            });
    }
}
