// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using MartinCostello.OpenApi.Services;
using MartinCostello.OpenApi.Utils;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds XML documentation for operations. This class cannot be inherited.
/// </summary>
/// <param name="descriptionService">A service for work with descriptions.</param>
internal sealed class AddOperationXmlDocumentationTransformer(IDescriptionService descriptionService)
    : IOpenApiOperationTransformer
{
    private readonly IDescriptionService _descriptionService = descriptionService;

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
        GetMethodInfo(context.Description) is not { } methodInfo
        || XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo) is not { Length: > 0 } xmlMethodName
            ? null
            : xmlMethodName;

    private static MethodInfo? GetMethodInfo(ApiDescription description)
    {
        if (description.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.MethodInfo;
        }

        return description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();
    }

    private static bool TryApplyParameterDescription(
        OpenApiOperation operation,
        ApiParameterDescription parameterDescription,
        string description)
    {
        var parameter = operation.Parameters.FirstOrDefault(p => p.Name == parameterDescription.Name);

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
        if (GetXmlMethodName(context) is not { Length: > 0 } xmlMethodName)
        {
            return;
        }

        if (operation.Summary is null
            && _descriptionService.GetDescription(xmlMethodName) is { Length: > 0 } methodSummary)
        {
            operation.Summary = methodSummary;
        }

        if (operation.Description is null
            && _descriptionService.GetDescription(xmlMethodName, section: "remarks") is { Length: > 0 } methodRemarks)
        {
            operation.Description = methodRemarks;
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

        foreach (var parameterDescription in context.Description.ParameterDescriptions)
        {
            if (TryApplyModelParameterDescription(operation, parameterDescription))
            {
                continue;
            }

            TryApplyEndpointParameterDescription(operation, context, parameterDescription);
        }
    }

    private bool TryApplyModelParameterDescription(
        OpenApiOperation operation,
        ApiParameterDescription parameterDescription)
    {
        if (parameterDescription.ParameterDescriptor is not IParameterInfoParameterDescriptor parameterDescriptor
            || XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(parameterDescriptor.ParameterInfo.Member)
                is not { Length: > 0 } xmlParameterName
            || _descriptionService.GetDescription(xmlParameterName) is not { Length: > 0 } parameterSummary)
        {
            return false;
        }

        return TryApplyParameterDescription(operation, parameterDescription, parameterSummary);
    }

    private bool TryApplyEndpointParameterDescription(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        ApiParameterDescription parameterDescription)
    {
        if (GetXmlMethodName(context) is { Length: > 0 } xmlMethodName
            && _descriptionService.GetDescription(xmlMethodName, parameterDescription.Name)
                is { Length: > 0 } parameterSummary)
        {
            return TryApplyParameterDescription(operation, parameterDescription, parameterSummary);
        }

        return false;
    }
}
