// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using MartinCostello.OpenApi.Services;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds XML documentation for OpenAPI operations. This class cannot be inherited.
/// </summary>
/// <param name="service">The <see cref="IDescriptionService"/> to use.</param>
internal sealed class AddOperationXmlDocumentationTransformer(IDescriptionService service) : IOpenApiOperationTransformer
{
    private readonly IDescriptionService _service = service;

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

    private static string? GetXmlMethodName(OpenApiOperationTransformerContext context) =>
        GetMethodInfo(context.Description) is not { } methodInfo ||
        XmlCommentsHelper.GetMemberNameForMethod(methodInfo) is not { Length: > 0 } name ? null : name;

    private static MethodInfo? GetMethodInfo(ApiDescription description)
    {
        if (description.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            return descriptor.MethodInfo;
        }

        return description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();
    }

    private static bool TryApplyParameterDescription(
        OpenApiOperation operation,
        ApiParameterDescription parameterDescription,
        string description)
    {
        var parameter = operation.Parameters.FirstOrDefault((p) => p.Name == parameterDescription.Name);

        if (parameter is null)
        {
            return false;
        }

        parameter.Description ??= description;

        return true;
    }

    private void ApplyOperationDescription(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context)
    {
        if (GetXmlMethodName(context) is not { Length: > 0 } methodName)
        {
            return;
        }

        if (operation.Summary is null &&
            _service.GetDescription(methodName) is { Length: > 0 } summary)
        {
            operation.Summary = summary;
        }

        if (operation.Description is null &&
            _service.GetDescription(methodName, section: "remarks") is { Length: > 0 } remarks)
        {
            operation.Description = remarks;
        }
    }

    private void ApplyParametersDescription(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        foreach (var description in context.Description.ParameterDescriptions)
        {
            if (TryApplyModelParameterDescription(operation, description))
            {
                continue;
            }

            TryApplyEndpointParameterDescription(operation, context, description);
        }
    }

    private bool TryApplyModelParameterDescription(
        OpenApiOperation operation,
        ApiParameterDescription description)
    {
        if (description.ParameterDescriptor is not IParameterInfoParameterDescriptor descriptor ||
            XmlCommentsHelper.GetMemberNameForFieldOrProperty(descriptor.ParameterInfo.Member) is not { Length: > 0 } name ||
            _service.GetDescription(name) is not { Length: > 0 } summary)
        {
            return false;
        }

        return TryApplyParameterDescription(operation, description, summary);
    }

    private bool TryApplyEndpointParameterDescription(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        ApiParameterDescription description)
    {
        if (GetXmlMethodName(context) is { Length: > 0 } methodName &&
            _service.GetDescription(methodName, description.Name) is { Length: > 0 } summary)
        {
            return TryApplyParameterDescription(operation, description, summary);
        }

        return false;
    }
}
