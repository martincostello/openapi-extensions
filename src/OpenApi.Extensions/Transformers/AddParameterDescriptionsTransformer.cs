// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

#pragma warning disable CA1852 // TODO Enable with .NET 9 preview 7

/// <summary>
/// A class that adds descriptions to OpenAPI operations. This class cannot be inherited.
/// </summary>
internal class AddParameterDescriptionsTransformer
{
    //// TODO Implement IOpenApiOperationTransformer
    //// TODO Make the class sealed
    //// TODO Remove virtual modifier

    public virtual Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (operation.Parameters is { Count: > 0 })
        {
            TryAddParameterDescriptions(operation.Parameters, context.Description);
        }

        return Task.CompletedTask;
    }

    private static void TryAddParameterDescriptions(
        IList<OpenApiParameter> parameters,
        ApiDescription description)
    {
        var arguments = description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .FirstOrDefault()?
            .GetParameters()
            .ToArray();

        if (arguments is { Length: > 0 })
        {
            foreach (var argument in arguments)
            {
                var attribute = argument
                    .GetCustomAttributes<DescriptionAttribute>()
                    .FirstOrDefault();

                if (attribute?.Description is { } value)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Description ??= value;
                    }
                }
            }
        }
    }
}
