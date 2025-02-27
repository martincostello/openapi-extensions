// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace MartinCostello.OpenApi.Transformers.Abstracts;

/// <summary>
/// A base class that helps in getting XML documentation.
/// </summary>
/// <param name="assembly">The assembly to get XML descriptions from.</param>
internal abstract class XmlTransformer(Assembly assembly)
{
    private readonly Assembly _assembly = assembly;
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private XPathNavigator? _navigator;

    protected Assembly Assembly => _assembly;

    protected string? GetDescription(string memberName)
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
