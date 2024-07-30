// Copyright (c) Martin Costello, 2024. All rights reserved.
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
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializationContext.Default);
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
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AnimalsJsonSerializationContext.Default);
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
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Insert(0, VehiclesJsonSerializationContext.Default);
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

    [Fact]
    public async Task Schema_Is_Correct_For_Example_Attribute_Hierarchy()
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
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AnimalsJsonSerializationContext.Default);
                });
            },
            (endpoints) =>
            {
                endpoints.MapGet("/no-example", Hierarchicy.NoExample);
                endpoints.MapGet("/example-is-attribute", Hierarchicy.ExampleIsAttribute);
                endpoints.MapPost("/example-is-parameter-attribute", Hierarchicy.ExampleIsParameterAttribute);
                endpoints.MapPost("/example-is-parameter-type", Hierarchicy.ExampleIsParameterType);
                endpoints.MapGet("/example-is-return-type", Hierarchicy.ExampleIsReturnType);
                endpoints.MapGet("/example-is-base-class-of-return-type", Hierarchicy.ExampleIsBaseClassOfReturnType);
                endpoints.MapGet("/example-is-return-type-overridden-with-attribute", Hierarchicy.ExampleIsReturnTypeOverriddenWithAttribute);
                endpoints.MapGet("/example-is-base-class-of-return-type-overridden-with-attribute", Hierarchicy.ExampleIsBaseClassOfReturnTypeOverriddenWithAttribute);
            });
    }

    private static class Hierarchicy
    {
        public static Cat NoExample()
        {
            return new Cat() { Name = "Garfield" };
        }

        [OpenApiExample<Cat, CatExampleProvider>]
        public static Cat ExampleIsAttribute()
        {
            return new Cat() { Name = "Nermal" };
        }

        public static Cat ExampleIsParameterAttribute([OpenApiExample<Cat, CatExampleProvider>] Cat cat)
        {
            return cat;
        }

        public static Dog ExampleIsParameterType(Dog dog)
        {
            return dog;
        }

        public static Dog ExampleIsReturnType()
        {
            return new Dog() { Name = "Snoopy" };
        }

        public static Spot ExampleIsBaseClassOfReturnType()
        {
            return new Spot() { Name = "Spot" };
        }

        [OpenApiExample<Dog, DogExampleProvider>]
        public static Dog ExampleIsReturnTypeOverriddenWithAttribute()
        {
            return new Dog() { Name = "Scooby Doo" };
        }

        [OpenApiExample<Spot, SpotExampleProvider>]
        public static Spot ExampleIsBaseClassOfReturnTypeOverriddenWithAttribute()
        {
            return new Spot() { Name = "Scooby Doo" };
        }

        private sealed class SpotExampleProvider : IExampleProvider<Spot>
        {
            public static Spot GenerateExample() => new() { Name = "Spot" };
        }
    }
}
