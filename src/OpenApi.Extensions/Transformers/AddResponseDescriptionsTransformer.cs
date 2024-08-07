// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds response descriptions to OpenAPI operations. This class cannot be inherited.
/// </summary>
internal sealed class AddResponseDescriptionsTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        foreach (var attribute in context.Description.ActionDescriptor.EndpointMetadata.OfType<OpenApiResponseAttribute>())
        {
            if (operation.Responses.TryGetValue(attribute.HttpStatusCode.ToString(CultureInfo.InvariantCulture), out var response))
            {
                response.Description = attribute.Description;
            }
        }

        return Task.CompletedTask;
    }
}
