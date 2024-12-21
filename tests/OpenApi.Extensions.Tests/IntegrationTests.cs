// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
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
                    options.SerializationContexts.Add(AppJsonSerializationContext.Default);

                    options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();
                    options.AddXmlComments<Greeting>();
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializationContext.Default);
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
    public async Task Schema_Is_Correct_For_App_As_Yaml()
    {
        // Arrange
        using var fixture = new TestFixture(
            (services) =>
            {
                services.AddHttpContextAccessor();
                services.AddOpenApi();
                services.AddOpenApiExtensions((options) =>
                {
                    options.AddExamples = true;
                    options.AddServerUrls = true;
                    options.SerializationContexts.Add(AppJsonSerializationContext.Default);

                    options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();
                    options.AddXmlComments<Greeting>();
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializationContext.Default);
                });
            },
            (endpoints) =>
            {
                endpoints.MapOpenApiYaml();
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
            },
            OutputHelper);

        // Act
        using var client = fixture.CreateDefaultClient();
        var actual = await client.GetStringAsync("/openapi/v1.yaml", TestContext.Current.CancellationToken);

        // Assert
        await Verify(actual, Settings);
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
                    options.SerializationContexts.Add(AnimalsJsonSerializationContext.Default);

                    options.AddExample<Animal, AnimalExampleProvider>();
                    options.AddExample<Dog>();

                    options.AddXmlComments<Animal>();

                    options.DescriptionTransformations.Add((p) => p.Replace(" Secret.", string.Empty, StringComparison.Ordinal));
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(AnimalsJsonSerializationContext.Default);
                });
            },
            (endpoints) =>
            {
                endpoints.MapPost("/register", (Animal animal) =>
                {
                    return TypedResults.Created();
                });

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
    public async Task Schema_Is_Correct_For_Interfaces()
    {
        // Act and Assert
        await VerifyOpenApiDocumentAsync(
            (services) =>
            {
                services.AddOpenApi();
                services.AddOpenApiExtensions((options) =>
                {
                    options.AddExamples = true;
                    options.SerializationContexts.Add(AnimalsJsonSerializationContext.Default);

                    options.AddXmlComments<IAnimal>();
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(AnimalsJsonSerializationContext.Default);
                });
            },
            (endpoints) =>
            {
                endpoints.MapPost("/register", (IAnimal animal) =>
                {
                    return TypedResults.Created();
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
                    options.SerializationContexts.Add(VehiclesJsonSerializationContext.Default);

                    options.AddXmlComments<Vehicle>();
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(VehiclesJsonSerializationContext.Default);
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
    public async Task Schema_Is_Correct_For_Not_Json()
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
                    options.SerializationContexts.Add(AppJsonSerializationContext.Default);
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializationContext.Default);
                });
            },
            (endpoints) =>
            {
                endpoints.MapGet("/greet", Greet);

                [return: OpenApiExample("Bonjour!")]
                static string Greet() => "Hello!";
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
                    options.AddExample<Car, CarExampleProvider>();
                    options.AddExample<Dog>();

                    options.SerializationContexts.Add(AnimalsJsonSerializationContext.Default);
                    options.SerializationContexts.Add(VehiclesJsonSerializationContext.Default);
                });
                services.ConfigureHttpJsonOptions((options) =>
                {
                    options.SerializerOptions.TypeInfoResolverChain.Add(AnimalsJsonSerializationContext.Default);
                    options.SerializerOptions.TypeInfoResolverChain.Add(VehiclesJsonSerializationContext.Default);
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
                endpoints.MapGet("/example-is-endpoint-metadata", Hierarchicy.NoExample).WithMetadata(new OpenApiExampleAttribute<Cat, CatExampleProvider>());

                endpoints.MapPost("/example-is-endpoint-metadata", Hierarchicy.NoExampleParameters)
                         .WithMetadata(new OpenApiExampleAttribute<Cat, CatExampleProvider>())
                         .WithMetadata(new OpenApiExampleAttribute("name"));

                endpoints.MapPost("/example-is-from-options", Hierarchicy.ExampleProviderFromOptions);
            });
    }

    [Fact]
    public async Task Schema_Is_Correct_For_Sample()
    {
        // Arrange
        using var fixture = new WebApplicationFactory<TodoApp.Program>()
            .WithWebHostBuilder((builder) => builder.ConfigureXUnitLogging(OutputHelper));

        // Act
        var actual = await fixture.GetOpenApiDocumentAsync();

        // Assert
        await VerifyJson(actual, Settings);
    }

    [Fact]
    public async Task Http_404_Is_Returned_If_Yaml_Document_Not_Found()
    {
        // Arrange
        using var fixture = new TestFixture(
            (services) => { },
            (endpoints) => endpoints.MapOpenApiYaml(),
            OutputHelper);

        // Act
        using var client = fixture.CreateDefaultClient();
        var actual = await client.GetAsync("/openapi/does-not-exist.yaml", TestContext.Current.CancellationToken);

        // Assert
        actual.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        await Verify(actual.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), Settings);
    }

    private static class Hierarchicy
    {
        public static Cat NoExample()
        {
            return new Cat() { Name = "Garfield" };
        }

        public static Cat? NoExampleParameters([FromBody] Cat cat, [FromQuery] string? name = null)
        {
            return name is { } ? cat : null;
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

        public static Car ExampleProviderFromOptions(Car car)
        {
            return car;
        }

        private sealed class SpotExampleProvider : IExampleProvider<Spot>
        {
            public static Spot GenerateExample() => new() { Name = "Spot" };
        }
    }

    private sealed class CarExampleProvider : IExampleProvider<Car>
    {
        public static Car GenerateExample() => new(CarType.Hatchback, 4, "MINI");
    }
}
