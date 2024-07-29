// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

#pragma warning disable CA1852 // TODO Enable with .NET 9 preview 7

/// <summary>
/// A class that adds response descriptions to OpenAPI operations. This class cannot be inherited.
/// </summary>
internal class AddResponseDescriptionsTransformer
{
    //// TODO Implement IOpenApiOperationTransformer
    //// TODO Make the class sealed
    //// TODO Remove virtual modifier

    public virtual Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var attributes = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<OpenApiResponseAttribute>()
            .ToArray();

        foreach (var attribute in attributes)
        {
            if (operation.Responses.TryGetValue(attribute.HttpStatusCode.ToString(CultureInfo.InvariantCulture), out var response))
            {
                response.Description = attribute.Description;
            }
        }

        return Task.CompletedTask;
    }
}
