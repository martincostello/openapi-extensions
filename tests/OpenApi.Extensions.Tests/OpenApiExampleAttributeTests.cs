// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Writers;

namespace MartinCostello.OpenApi;

public static partial class OpenApiExampleAttributeTests
{
    [Fact]
    public static void OpenApiExampleAttribute_Constructor_Initializes_Instance()
    {
        // Arrange
        var value = "example";

        // Act
        var actual = new OpenApiExampleAttribute(value);

        // Assert
        actual.Value.ShouldBe(value);
        actual.GenerateExample().ShouldBe(value);
    }

    [Fact]
    public static void OpenApiExampleAttribute_IExampleProvider_Implementation_Is_Correct()
    {
        // Act
        var actual = GetExample<string, OpenApiExampleAttribute>();

        // Assert
        actual.ShouldBe("value");
    }

    [Fact]
    public static void OpenApiExampleAttributeT1_Constructor_Initializes_Instance()
    {
        // Act
        var actual = new OpenApiExampleAttribute<Human>();

        // Assert
        actual.ExampleType.ShouldBe(typeof(Human));
    }

    [Fact]
    public static void OpenApiExampleAttributeT1_GenerateExample_Returns_Correct_Value()
    {
        // Arrange
        var target = new OpenApiExampleAttribute<Human>();

        // Act
        var actual = target.GenerateExample();

        // Assert
        actual.ShouldNotBeNull();
        actual.Name.ShouldBe("Martin");
    }

    [Fact]
    public static void OpenApiExampleAttributeT2_Constructor_Initializes_Instance()
    {
        // Act
        var actual = new OpenApiExampleAttribute<Person, PersonExampleProvider>();

        // Assert
        actual.ExampleType.ShouldBe(typeof(Person));
    }

    [Fact]
    public static void OpenApiExampleAttributeT2_GenerateExample_Returns_Correct_Value()
    {
        // Arrange
        var target = new OpenApiExampleAttribute<Person, PersonExampleProvider>();

        // Act
        var actual = target.GenerateExample();

        // Assert
        actual.ShouldNotBeNull();
        actual.Name.ShouldBe("Martin");
    }

    [Theory]
    [InlineData(false, "Name")]
    [InlineData(true, "name")]
    public static void OpenApiExampleAttributeT2_GenerateExample_As_IOpenApiExampleMetadata_Returns_Correct_Value(
        bool camelCase,
        string expectedPropertyName)
    {
        // Arrange
        IOpenApiExampleMetadata target = new OpenApiExampleAttribute<Person, PersonExampleProvider>();
        JsonSerializerContext context = camelCase ? PersonJsonSerializationContextCamel.Default : PersonJsonSerializationContextPascal.Default;

        // Act
        var actual = target.GenerateExample(context);

        // Assert
        actual.ShouldNotBeNull();

        // Arrange
        using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);

        // Act
        actual.Write(jsonWriter, Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);

        // Assert
        using var document = JsonDocument.Parse(stringWriter.ToString());

        document.ShouldNotBeNull();
        document.RootElement.ValueKind.ShouldBe(JsonValueKind.Object);
        document.RootElement.EnumerateObject().Count().ShouldBe(1);
        document.RootElement.TryGetProperty(expectedPropertyName, out var value).ShouldBeTrue();

        value.ValueKind.ShouldBe(JsonValueKind.String);
        value.GetString().ShouldBe("Martin");
    }

    private static TSchema GetExample<TSchema, TProvider>()
        where TProvider : IExampleProvider<TSchema>
        => TProvider.GenerateExample();

    private sealed record Human(string Name) : IExampleProvider<Human>
    {
        public static Human GenerateExample() => new("Martin");
    }

    private sealed record Person(string Name);

    private sealed class PersonExampleProvider : IExampleProvider<Person>
    {
        public static Person GenerateExample() => new("Martin");
    }

    [JsonSerializable(typeof(Person))]
    [JsonSourceGenerationOptions(
        NumberHandling = JsonNumberHandling.Strict,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    private sealed partial class PersonJsonSerializationContextCamel : JsonSerializerContext;

    [JsonSerializable(typeof(Person))]
    [JsonSourceGenerationOptions(
        NumberHandling = JsonNumberHandling.Strict,
        PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
    private sealed partial class PersonJsonSerializationContextPascal : JsonSerializerContext;
}
