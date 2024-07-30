// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace MartinCostello.OpenApi;

internal static class ReflectionExtensions
{
    public static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(this MethodInfo method)
        => method.GetCustomAttributes()
                 .OfType<IOpenApiExampleMetadata>()
                 .Concat(method.ReturnParameter.GetExampleMetadata());

    public static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(this ParameterInfo parameter)
        => parameter.GetCustomAttributes().OfType<IOpenApiExampleMetadata>();

    public static IOpenApiExampleMetadata? GetExampleMetadata(this Type type)
        => type.GetCustomAttributes(inherit: true)
               .OfType<IOpenApiExampleMetadata>()
               .FirstOrDefault((p) => p.ExampleType.IsAssignableFrom(type));
}
