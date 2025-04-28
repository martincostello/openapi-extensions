// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class representing an operation and schema description transformer. This class cannot be inherited.
/// </summary>
/// <param name="transformer">A delegate to a method to use to transform description strings.</param>
internal sealed class DescriptionsTransformer(Func<string, string> transformer) :
    IOpenApiOperationTransformer,
    IOpenApiSchemaTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ApplyResponseDescriptions(operation);
        ApplyParametersDescriptions(operation);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Description is { } schemaDescription)
        {
            schema.Description = transformer(schemaDescription);
        }

        if (schema.Properties is { } properties)
        {
            foreach (var property in properties.Values)
            {
                if (property.Description is { } propertyDescription)
                {
                    property.Description = transformer(propertyDescription);
                }
            }
        }

        return Task.CompletedTask;
    }

    internal static string RemoveBackticks(string description)
        => description.Replace("`", string.Empty, StringComparison.Ordinal);

    internal static string RemoveStyleCopPrefixes(string description)
    {
        string[] prefixes =
        [
            "Gets or sets a value indicating ",
            "Gets a value indicating ",
            "Gets or sets ",
            "Gets ",
        ];

        foreach (var prefix in prefixes)
        {
            description = description.Replace(prefix, string.Empty, StringComparison.Ordinal);
        }

        description = char.ToUpperInvariant(description[0]) + description[1..];

        return description;
    }

    private void ApplyResponseDescriptions(OpenApiOperation operation)
    {
        if (operation.Responses is { } responses)
        {
            foreach (var response in responses.Values)
            {
                if (response.Content is not { } content)
                {
                    continue;
                }

                foreach (var model in content.Values)
                {
                    if (model.Schema?.Properties is { } properties)
                    {
                        foreach (var property in properties.Values)
                        {
                            if (property.Description is { } description)
                            {
                                property.Description = transformer(description);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ApplyParametersDescriptions(OpenApiOperation operation)
    {
        if (operation.Parameters is { } parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Description is { } description)
                {
                    parameter.Description = transformer(description);
                }
            }
        }
    }
}
