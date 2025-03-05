// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using MartinCostello.OpenApi.Utils;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds XML documentation for operations. This class cannot be inherited.
/// </summary>
/// <param name="assembly">The assembly to add XML descriptions to the operations of.</param>
internal sealed class AddOperationXmlDocumentationTransformer(Assembly assembly) :
    XmlTransformer(assembly),
    IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ApplyOperationDescription(operation, context);
        ApplyParametersDescription(operation, context);

        return Task.CompletedTask;
    }

    private static MethodInfo? TryGetMethodInfo(ApiDescription description)
        => description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();

    private void ApplyOperationDescription(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context)
    {
        if (TryGetMethodInfo(context.Description) is not { } methodInfo)
        {
            return;
        }

        if (XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo) is { Length: > 0 } xmlMethodName
            && GetDescription(xmlMethodName) is { Length: > 0 } methodSummary)
        {
            operation.Summary = methodSummary;
        }
    }

    private void ApplyParametersDescription(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context)
    {
        foreach (var contextParameterDescription in context.Description.ParameterDescriptions)
        {
            if (contextParameterDescription.ParameterDescriptor is not IParameterInfoParameterDescriptor parameterDescriptor
                || XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(parameterDescriptor.ParameterInfo.Member)
                    is not { Length: > 0 } xmlParameterName
                || GetDescription(xmlParameterName) is not { Length: > 0 } parameterDescription)
            {
                continue;
            }

            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == contextParameterDescription.Name);

            if (parameter is not null)
            {
                parameter.Description = parameterDescription;
            }
        }
    }
}
