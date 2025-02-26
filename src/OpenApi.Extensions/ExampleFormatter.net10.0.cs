// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#if NET10_0_OR_GREATER

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class containing methods to help format JSON examples for OpenAPI. This class cannot be inherited.
/// </summary>
internal static partial class ExampleFormatter
{
    /// <summary>
    /// Formats the specified value as JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="example">The example value to format as JSON.</param>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The <see cref="JsonNode"/> to use as the example.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static JsonNode AsJson<T>(T example, JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? json = JsonSerializer.Serialize(example, typeof(T), context);
        using var document = JsonDocument.Parse(json);

        if (!TryParse(document.RootElement, out var any) || any is null)
        {
            any = new JsonObject();
        }

        return any;
    }

    private static bool TryParse(JsonElement token, out JsonNode? any)
    {
        any = null;

        switch (token.ValueKind)
        {
            case JsonValueKind.Array:
                var array = new JsonArray();

                foreach (var value in token.EnumerateArray())
                {
                    if (TryParse(value, out var child))
                    {
                        array.Add(child);
                    }
                }

                any = array;
                return true;

            case JsonValueKind.False:
                any = JsonValue.Create(false);
                return true;

            case JsonValueKind.True:
                any = JsonValue.Create(true);
                return true;

            case JsonValueKind.Number:
                any = JsonValue.Create(token.GetDouble());
                return true;

            case JsonValueKind.String:
                any = JsonValue.Create(token.GetString());
                return true;

            case JsonValueKind.Object:
                var obj = new JsonObject();

                foreach (var child in token.EnumerateObject())
                {
                    if (TryParse(child.Value, out var value))
                    {
                        obj[child.Name] = value;
                    }
                }

                any = obj;
                return true;

            case JsonValueKind.Null:
                any = JsonValue.Create(new bool?());
                return true;

            case JsonValueKind.Undefined:
            default:
                return false;
        }
    }
}

#endif
