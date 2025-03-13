// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace MartinCostello.OpenApi;

internal sealed class XmlDescriptionService(Assembly assembly)
{
    private readonly Assembly _assembly = assembly;
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private XPathNavigator? _navigator;

    public string? GetDescription(string memberName, string? parameterName = null, string? section = "summary")
    {
        var cacheKey =
            memberName +
            (!string.IsNullOrEmpty(parameterName) ? $"/{parameterName}" : $"/{section}");

        if (_descriptions.TryGetValue(cacheKey, out var description))
        {
            return description;
        }

        var navigator = CreateNavigator();

        var xmlPath =
            !string.IsNullOrEmpty(parameterName) ?
            $"/doc/members/member[@name='{memberName}']/param[@name='{parameterName}']" :
            $"/doc/members/member[@name='{memberName}']/{section}";

        if (navigator.SelectSingleNode(xmlPath) is { Value.Length: > 0 } node)
        {
            description = node.Value.Trim();
        }

        _descriptions[cacheKey] = description;

        return description;
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
