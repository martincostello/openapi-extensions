// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#if NET9_0

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.OpenApi;

internal static partial class ExampleFormatter
{
    /// <summary>
    /// Formats the specified value as JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="example">The example value to format as JSON.</param>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The <see cref="IOpenApiAny"/> to use as the example.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static IOpenApiAny AsJson<T>(T example, JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? json = JsonSerializer.Serialize(example, typeof(T), context);
        using var document = JsonDocument.Parse(json);

        return TryParse(document.RootElement, out var any) ? any : new OpenApiNull();
    }

    private static bool TryParse(JsonElement token, [NotNullWhen(true)] out IOpenApiAny? any)
    {
        any = null;

        switch (token.ValueKind)
        {
            case JsonValueKind.Array:
                var array = new OpenApiArray();

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
                any = new OpenApiBoolean(false);
                return true;

            case JsonValueKind.True:
                any = new OpenApiBoolean(true);
                return true;

            case JsonValueKind.Number:
                any = new OpenApiDouble(token.GetDouble());
                return true;

            case JsonValueKind.String:
                any = new OpenApiString(token.GetString());
                return true;

            case JsonValueKind.Object:
                var obj = new OpenApiObject();

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
                any = new OpenApiNull();
                return true;

            case JsonValueKind.Undefined:
            default:
                return false;
        }
    }
}
#endif
