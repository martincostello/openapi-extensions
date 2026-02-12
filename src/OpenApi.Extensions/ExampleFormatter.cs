// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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
    /// The <see cref="JsonNode"/> to use as the example, if any.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public static JsonNode? AsJson<T>(T example, JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? json = JsonSerializer.Serialize(example, typeof(T), context);
        return JsonValue.Parse(json);
    }
}
