﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

#pragma warning disable CA1852 // TODO Enable with .NET 9 preview 7

/// <summary>
/// A class representing an operation and schema description transformer. This class cannot be inherited.
/// </summary>
/// <param name="transformer">A delegate to a method to use to transform description strings.</param>
internal class DescriptionsTransformer(Func<string, string> transformer)
{
    //// TODO Implement IOpenApiOperationTransformer and IOpenApiSchemaTransformer
    //// TODO Make the class sealed
    //// TODO Remove virtual modifiers

    public virtual Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var response in operation.Responses.Values)
        {
            foreach (var model in response.Content.Values)
            {
                foreach (var property in model.Schema.Properties.Values)
                {
                    if (property.Description is { } description)
                    {
                        property.Description = transformer(description);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Description is { } schemaDescription)
        {
            schema.Description = transformer(schemaDescription);
        }

        foreach (var property in schema.Properties.Values)
        {
            if (property.Description is { } description)
            {
                property.Description = transformer(description);
            }
        }

        return Task.CompletedTask;
    }

    internal static string RemoveBackticks(string description)
        => description.Replace("`", string.Empty, StringComparison.Ordinal);

    internal static string RemoveStyleCopPrefixes(string description)
    {
        const string Prefix = "Gets or sets ";

        description = description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
        description = char.ToUpperInvariant(description[0]) + description[1..];

        return description;
    }
}