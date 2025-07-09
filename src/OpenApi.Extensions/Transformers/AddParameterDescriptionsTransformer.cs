// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;

#if NET9_0
using Microsoft.OpenApi.Models;
#else
using Microsoft.OpenApi;
#endif

#if NET10_0_OR_GREATER
using OpenApiParameter = Microsoft.OpenApi.IOpenApiParameter;
#endif

#if NET10_0_OR_GREATER
using OpenApiParameter = Microsoft.OpenApi.Models.Interfaces.IOpenApiParameter;
#endif

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds descriptions to OpenAPI operations. This class cannot be inherited.
/// </summary>
internal sealed class AddParameterDescriptionsTransformer : IOpenApiOperationTransformer
{
    private readonly ConcurrentDictionary<MethodInfo, ParameterDescription[]> _methodParametersDescriptions = [];

    /// <inheritdoc/>
    public Task TransformAsync(
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

    private void TryAddParameterDescriptions(
        IList<OpenApiParameter> parameters,
        ApiDescription apiDescription)
    {
        var descriptions = GetMethodParameterDescriptions(apiDescription);

        if (descriptions is { Length: > 0 })
        {
            foreach ((var argument, var description) in descriptions)
            {
                if (description is not null)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Description ??= description;
                    }
                }
            }
        }
    }

    private ParameterDescription[] GetMethodParameterDescriptions(ApiDescription description)
    {
        var method = description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();

        if (method is null)
        {
            return [];
        }

        return _methodParametersDescriptions.GetOrAdd(method, static (p) =>
        {
            var parameters = p.GetParameters();
            var descriptions = new ParameterDescription[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var description = p.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault()?.Description;

                descriptions[i] = new(parameter, description);
            }

            return descriptions;
        });
    }

    private sealed record ParameterDescription(ParameterInfo Parameter, string? Description);
}
