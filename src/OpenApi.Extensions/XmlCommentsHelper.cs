// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class that helps in building node names for XML.
/// </summary>
internal static class XmlCommentsHelper
{
    public static string? GetMemberNameForMethod(MethodInfo method)
    {
        var builder = new StringBuilder("M:");

        if (method.DeclaringType is null)
        {
            return null;
        }

        builder.Append(QualifiedNameFor(method.DeclaringType));
        builder.Append(CultureInfo.InvariantCulture, $".{method.Name}");

        var parameters = method.GetParameters();
        if (parameters.Length > 0)
        {
            var parametersNames = parameters.Select(p =>
                p.ParameterType.IsGenericParameter
                    ? $"`{p.ParameterType.GenericParameterPosition}"
                    : QualifiedNameFor(p.ParameterType, expandGenericArgs: true))
                .ToArray();

            if (parametersNames.Any(p => p is null))
            {
                return null;
            }

            builder.Append(CultureInfo.InvariantCulture, $"({string.Join(",", parametersNames)})");
        }

        return builder.ToString();
    }

    public static string? GetMemberNameForFieldOrProperty(MemberInfo fieldOrPropertyInfo)
    {
        if (fieldOrPropertyInfo.DeclaringType is null)
        {
            return null;
        }

        var builder = new StringBuilder((fieldOrPropertyInfo.MemberType & MemberTypes.Field) != 0 ? "F:" : "P:");
        builder.Append(QualifiedNameFor(fieldOrPropertyInfo.DeclaringType));
        builder.Append(CultureInfo.InvariantCulture, $".{fieldOrPropertyInfo.Name}");

        return builder.ToString();
    }

    private static string? QualifiedNameFor(Type type, bool expandGenericArgs = false)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();

            if (elementType is null)
            {
                return null;
            }

            return elementType.IsGenericParameter
                ? $"`{elementType.GenericParameterPosition}[]"
                : $"{QualifiedNameFor(elementType, expandGenericArgs)}[]";
        }

        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(type.Namespace))
        {
            builder.Append(CultureInfo.InvariantCulture, $"{type.Namespace}.");
        }

        if (type.IsNested)
        {
            builder.Append(CultureInfo.InvariantCulture, $"{string.Join(".", GetNestedTypeNames(type))}.");
        }

        if (type.IsConstructedGenericType && expandGenericArgs)
        {
            var nameSansGenericArgs = type.Name.Split('`').First();
            builder.Append(nameSansGenericArgs);

            var genericArgsNames = type.GetGenericArguments().Select(t => t.IsGenericParameter
                ? $"`{t.GenericParameterPosition}"
                : QualifiedNameFor(t, true));

            builder.Append(CultureInfo.InvariantCulture, $"{{{string.Join(",", genericArgsNames)}}}");
        }
        else
        {
            builder.Append(type.Name);
        }

        return builder.ToString();
    }

    private static IEnumerable<string> GetNestedTypeNames(Type type)
    {
        if (!type.IsNested || type.DeclaringType == null)
        {
            yield break;
        }

        foreach (var nestedTypeName in GetNestedTypeNames(type.DeclaringType))
        {
            yield return nestedTypeName;
        }

        yield return type.DeclaringType.Name;
    }
}
