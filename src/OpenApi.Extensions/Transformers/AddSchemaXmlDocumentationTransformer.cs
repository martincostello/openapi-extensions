// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds XML documentation to OpenAPI schemas. This class cannot be inherited.
/// </summary>
/// <param name="assembly">The assembly to add XML documentation to the types of.</param>
/// <param name="service">The <see cref="IDescriptionService"/> to use.</param>
internal sealed class AddSchemaXmlDocumentationTransformer(Assembly assembly, XmlDescriptionService service)
    : IOpenApiSchemaTransformer
{
    private readonly Assembly _assembly = assembly;
    private readonly XmlDescriptionService _service = service;

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Description is null &&
            GetMemberName(context.JsonTypeInfo, context.JsonPropertyInfo) is { Length: > 0 } memberName &&
            _service.GetDescription(memberName) is { Length: > 0 } description)
        {
            schema.Description = description;
        }

        return Task.CompletedTask;
    }

    private string? GetMemberName(JsonTypeInfo typeInfo, JsonPropertyInfo? propertyInfo)
    {
        if (typeInfo.Type.Assembly != _assembly &&
            propertyInfo?.DeclaringType.Assembly != _assembly)
        {
            return null;
        }
        else if (propertyInfo is not null)
        {
            var typeName = propertyInfo.DeclaringType.FullName;
            var memberName =
                propertyInfo.AttributeProvider is MemberInfo member ?
                member.Name :
                $"{char.ToUpperInvariant(propertyInfo.Name[0])}{propertyInfo.Name[1..]}";

            var memberType = propertyInfo.AttributeProvider is PropertyInfo ? "P" : "F";

            return $"{memberType}:{typeName}{Type.Delimiter}{memberName}";
        }
        else
        {
            return $"T:{typeInfo.Type.FullName}";
        }
    }
}
