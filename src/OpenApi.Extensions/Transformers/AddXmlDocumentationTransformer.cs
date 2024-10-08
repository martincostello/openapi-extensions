﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds XML documentation descriptions to OpenAPI schemas. This class cannot be inherited.
/// </summary>
/// <param name="assembly">The assembly to add XML descriptions to the types of.</param>
internal sealed class AddXmlDocumentationTransformer(Assembly assembly) : IOpenApiSchemaTransformer
{
    private readonly Assembly _assembly = assembly;
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private XPathNavigator? _navigator;

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Description is null &&
            GetMemberName(context.JsonTypeInfo, context.JsonPropertyInfo) is { Length: > 0 } memberName &&
            GetDescription(memberName) is { Length: > 0 } description)
        {
            schema.Description = description;
        }

        return Task.CompletedTask;
    }

    private string? GetDescription(string memberName)
    {
        if (_descriptions.TryGetValue(memberName, out var description))
        {
            return description;
        }

        var navigator = CreateNavigator();
        var node = navigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");

        if (node is not null)
        {
            description = node.Value.Trim();
        }

        _descriptions[memberName] = description;

        return description;
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

    private XPathNavigator CreateNavigator()
    {
        if (_navigator is null)
        {
            var path = Path.Combine(AppContext.BaseDirectory, $"{_assembly.GetName().Name}.xml");
            using var reader = XmlReader.Create(path);
            _navigator = new XPathDocument(reader).CreateNavigator();
        }

        return _navigator;
    }
}
