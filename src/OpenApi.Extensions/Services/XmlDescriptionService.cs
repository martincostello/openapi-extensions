// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace MartinCostello.OpenApi.Services;

/// <summary>
/// Represents a service for work with XML descriptions.
/// </summary>
/// <param name="assembly">The assembly to search XML descriptions from.</param>
internal class XmlDescriptionService(Assembly assembly) : IDescriptionService
{
    private readonly Assembly _assembly = assembly;
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private XPathNavigator? _navigator;

    /// <inheritdoc/>
    public string? GetDescription(string memberName, string? parameterName = null)
    {
        var cacheKey = memberName +
                             (!string.IsNullOrEmpty(parameterName)
                                 ? $"/{parameterName}"
                                 : string.Empty);

        if (_descriptions.TryGetValue(cacheKey, out var description))
        {
            return description;
        }

        var navigator = CreateNavigator();
        var xmlPath = !string.IsNullOrEmpty(parameterName)
            ? $"/doc/members/member[@name='{memberName}']/param[@name='{parameterName}']"
            : $"/doc/members/member[@name='{memberName}']/summary";
        var node = navigator.SelectSingleNode(xmlPath);

        if (node is not null)
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
