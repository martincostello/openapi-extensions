// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;

namespace MartinCostello.OpenApi;

internal static class XmlCommentsHelper
{
    public static string? GetMemberName(MethodInfo method)
    {
        if (method.DeclaringType is null)
        {
            return null;
        }

        var builder = new StringBuilder()
            .Append("M:")
            .Append(QualifiedNameFor(method.DeclaringType))
            .Append('.')
            .Append(method.Name);

        if (method.GetParameters() is { Length: > 0 } parameters)
        {
            string?[] names = [.. parameters.Select(GetName)];

            if (names.Any((p) => p is null))
            {
                return null;
            }

            builder.Append('(')
                   .Append(string.Join(',', names))
                   .Append(')');
        }

        return builder.ToString();

        static string? GetName(ParameterInfo parameter)
            => GetNameForType(parameter.ParameterType);
    }

    public static string? GetMemberName(MemberInfo member)
    {
        if (member.DeclaringType is null)
        {
            return null;
        }

        var builder = new StringBuilder()
            .Append((member.MemberType & MemberTypes.Field) != 0 ? 'F' : 'P')
            .Append(':')
            .Append(QualifiedNameFor(member.DeclaringType))
            .Append('.')
            .Append(member.Name);

        return builder.ToString();
    }

    private static string? QualifiedNameFor(Type type, bool expandGenericArguments = false)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();

            if (elementType is null)
            {
                return null;
            }

            return GetNameForType(elementType) + "[]";
        }

        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(type.Namespace))
        {
            builder.Append(type.Namespace)
                   .Append('.');
        }

        if (type.IsNested)
        {
            builder.Append(string.Join('.', GetNestedTypeNames(type)))
                   .Append('.');
        }

        if (type.IsConstructedGenericType && expandGenericArguments)
        {
            int index = type.Name.IndexOf('`', StringComparison.Ordinal);

            Debug.Assert(index is not -1, "Backtick was not found in generic type name.");

            builder.Append(type.Name.AsSpan(..index));

            var argumentNames = type
                .GetGenericArguments()
                .Select(GetNameForType);

            builder.Append('{')
                   .Append(string.Join(',', argumentNames))
                   .Append('}');
        }
        else
        {
            builder.Append(type.Name);
        }

        return builder.ToString();
    }

    private static IEnumerable<string> GetNestedTypeNames(Type type)
    {
        if (!type.IsNested || type.DeclaringType is null)
        {
            yield break;
        }

        foreach (var name in GetNestedTypeNames(type.DeclaringType))
        {
            yield return name;
        }

        yield return type.DeclaringType.Name;
    }

    private static string? GetNameForType(Type type)
    {
        return type.IsGenericParameter ?
            FormattableString.Invariant($"`{type.GenericParameterPosition}") :
            QualifiedNameFor(type, expandGenericArguments: true);
    }
}
