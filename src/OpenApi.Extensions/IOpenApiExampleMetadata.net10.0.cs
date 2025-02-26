// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#if NET10_0_OR_GREATER

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MartinCostello.OpenApi;

public partial interface IOpenApiExampleMetadata
{
    /// <summary>
    /// Generates an example object for the type associated with <see cref="ExampleType"/>.
    /// </summary>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The example to use.
    /// </returns>
    JsonNode? GenerateExample(JsonSerializerContext context);
}

#endif
