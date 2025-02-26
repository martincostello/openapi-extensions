// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#if NET9_0

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.OpenApi;

public partial class OpenApiExampleAttribute<TSchema, TProvider> : Attribute, IOpenApiExampleMetadata
    where TProvider : IExampleProvider<TSchema>
{
    /// <inheritdoc/>
    IOpenApiAny IOpenApiExampleMetadata.GenerateExample(JsonSerializerContext context)
        => ExampleFormatter.AsJson(GenerateExample(), context);
}

#endif
