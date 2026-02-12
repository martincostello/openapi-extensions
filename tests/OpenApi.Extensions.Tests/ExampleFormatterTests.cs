// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.OpenApi;

public static partial class ExampleFormatterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Can_Serialize_Boolean(bool value)
    {
        // Act
        var actual = Serialize(value);

        // Assert
        await VerifyJson(actual).UseParameters(value).UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public static async Task Can_Serialize_Integer()
    {
        // Arrange
        int value = 42;

        // Act
        var actual = Serialize(value);

        // Assert
        await VerifyJson(actual).UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public static async Task Can_Serialize_Long()
    {
        // Arrange
        long value = 42;

        // Act
        var actual = Serialize(value);

        // Assert
        await VerifyJson(actual).UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public static async Task Can_Serialize_String()
    {
        // Arrange
        string value = "hello world";

        // Act
        var actual = Serialize(value);

        // Assert
        await VerifyJson(actual).UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public static async Task Can_Serialize_Null_String()
    {
        // Arrange
        string? value = null;

        // Act
        var actual = Serialize(value);

        // Assert
        await VerifyJson(actual).UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public static async Task Can_Serialize_Complex_Object()
    {
        // Arrange
        var value = new Custom()
        {
            Boolean = true,
            DateTime = new DateTime(2024, 7, 1, 12, 34, 56, DateTimeKind.Utc),
            DateTimeOffset = new DateTimeOffset(2024, 7, 1, 12, 34, 56, TimeSpan.Zero),
            Integer = 42,
            Long = 42,
            String = "hello world",
            Child = new Custom()
            {
                Boolean = false,
                DateTime = new DateTime(2024, 6, 1, 12, 34, 56, DateTimeKind.Utc),
                DateTimeOffset = new DateTimeOffset(2024, 6, 1, 12, 34, 56, TimeSpan.Zero),
                Integer = int.MaxValue,
                Long = long.MaxValue,
                String = "nested",
            },
            Children =
            [
                new()
                {
                    Boolean = true,
                    DateTime = new DateTime(2024, 5, 1, 12, 34, 56, DateTimeKind.Utc),
                    DateTimeOffset = new DateTimeOffset(2024, 5, 1, 12, 34, 56, TimeSpan.Zero),
                    Integer = int.MinValue,
                    Long = long.MinValue,
                    String = "first",
                },
                new()
                {
                    Boolean = false,
                    DateTime = new DateTime(2024, 4, 1, 12, 34, 56, DateTimeKind.Utc),
                    DateTimeOffset = new DateTimeOffset(2024, 4, 1, 12, 34, 56, TimeSpan.Zero),
                    Integer = 0,
                    Long = 0,
                    String = "second",
                },
            ],
        };

        // Act
        var actual = Serialize(value);

        // Assert
        await VerifyJson(actual, DocumentTests.Settings).UniqueForTargetFrameworkAndVersion();
    }

    private static string? Serialize<T>(T value)
    {
        var actual = ExampleFormatter.AsJson(value, ReflectionJsonSerializerContext.Default);
        return actual?.ToJsonString();
    }

    private sealed class Custom
    {
        public bool Boolean { get; set; }

        public int Integer { get; set; }

        public long Long { get; set; }

        public string String { get; set; } = default!;

        public DateTime DateTime { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public Custom Child { get; set; } = default!;

        public IList<Custom> Children { get; set; } = [];
    }

    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(DateTimeOffset))]
    [JsonSerializable(typeof(Custom))]
    [JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.Strict)]
    private sealed partial class ReflectionJsonSerializerContext : JsonSerializerContext;
}
