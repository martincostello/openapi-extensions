// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.OpenApi;

/// <summary>
/// Defines a metadata for an OpenAPI example.
/// </summary>
public interface IOpenApiExampleMetadata
{
    /// <summary>
    /// Gets the type associated with the example.
    /// </summary>
    Type ExampleType { get; }

    /// <summary>
    /// Generates an example object for the type associated with <see cref="ExampleType"/>.
    /// </summary>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The example to use.
    /// </returns>
    IOpenApiAny GenerateExample(JsonSerializerContext context);
}
