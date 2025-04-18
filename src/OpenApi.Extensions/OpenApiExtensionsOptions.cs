﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json.Serialization;
using MartinCostello.OpenApi.Transformers;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class representing the options for the OpenAPI extensions.
/// </summary>
public class OpenApiExtensionsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to add examples to OpenAPI operations and schemas.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/>.
    /// </remarks>
    public bool AddExamples { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to add the server URLs to the OpenAPI document.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/>.
    /// </remarks>
    public bool AddServerUrls { get; set; }

    /// <summary>
    /// Gets or sets the default server URL to use if no server URLs are available,
    /// such as when generating static OpenAPI documents using the
    /// <a href="https://www.nuget.org/packages/Microsoft.Extensions.ApiDescription.Server">Microsoft.Extensions.ApiDescription.Server</a>
    /// NuGet package.
    /// </summary>
    public string? DefaultServerUrl { get; set; }

    /// <summary>
    /// Gets the transformation(s) to apply to the descriptions of operations and/or schemas, if any.
    /// </summary>
    /// <remarks>
    /// The default transformations are listed below.
    /// <list type="number">
    ///    <item>
    ///        <term>Remove backticks</term>
    ///        <description>Removes any <c>`</c> characters from the description.</description>
    ///    </item>
    ///    <item>
    ///        <term>Remove StyleCop prefixes</term>
    ///        <description>Removes the prefix &quot;Gets or sets a value &quot; from the description.</description>
    ///    </item>
    /// </list>
    /// </remarks>
    public IList<Func<string, string>> DescriptionTransformations { get; } =
    [
        DescriptionsTransformer.RemoveBackticks,
        DescriptionsTransformer.RemoveStyleCopPrefixes,
    ];

    /// <summary>
    /// Gets a collection of metadata to use when adding examples to OpenAPI operations and schemas.
    /// </summary>
    /// <remarks>
    /// By default example metadata is searched for in the metadata for endpoints, as well as on
    /// their parameters and parameter and return types. This collection can be used to provide examples
    /// for types and/or endpoints where the examples cannot be inferred from the metadata.
    /// <para/>
    /// For example, this property can be used to add metadata for types for code you cannot change
    /// the definition of, such as types from a third-party library or ASP.NET Core itself.
    /// </remarks>
    public ICollection<IOpenApiExampleMetadata> ExamplesMetadata { get; } = [];

    /// <summary>
    /// Gets the instances of <see cref="JsonSerializerContext"/> to use
    /// when <see cref="AddExamples"/> is <see langword="true"/>.
    /// </summary>
    public IList<JsonSerializerContext> SerializationContexts { get; } = [];

    /// <summary>
    /// Gets the assemblies that should be used to find XML documentation
    /// to provide descriptions for operations and/or schemas, if any.
    /// </summary>
    public IList<Assembly> XmlDocumentationAssemblies { get; } = [];

    /// <summary>
    /// Adds the specified type as its own example provider to the options.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema.</typeparam>
    /// <returns>
    /// The current instance of <see cref="OpenApiExtensionsOptions"/>.
    /// </returns>
    public OpenApiExtensionsOptions AddExample<TSchema>()
        where TSchema : IExampleProvider<TSchema>
        => AddExample<TSchema, TSchema>();

    /// <summary>
    /// Adds an example provider for the specified type to the options.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema.</typeparam>
    /// <typeparam name="TProvider">The type of the example provider.</typeparam>
    /// <returns>
    /// The current instance of <see cref="OpenApiExtensionsOptions"/>.
    /// </returns>
    public OpenApiExtensionsOptions AddExample<TSchema, TProvider>()
        where TProvider : IExampleProvider<TSchema>
    {
        ExamplesMetadata.Add(new OpenApiExampleAttribute<TSchema, TProvider>());
        return this;
    }

    /// <summary>
    /// Adds XML documentation for the assembly associated with the specified type to the options.
    /// </summary>
    /// <typeparam name="T">The type to add XML documentation for.</typeparam>
    /// <returns>
    /// The current instance of <see cref="OpenApiExtensionsOptions"/>.
    /// </returns>
    public OpenApiExtensionsOptions AddXmlComments<T>()
    {
        XmlDocumentationAssemblies.Add(typeof(T).Assembly);
        return this;
    }

    internal Func<string, string>? GetDescriptionTransformer()
    {
        if (DescriptionTransformations.Count == 0)
        {
            return null;
        }

        var transformer = DescriptionTransformations[0];

        if (DescriptionTransformations.Count > 1)
        {
            transformer = (description) =>
            {
                string transformed = description;

                foreach (var transformer in DescriptionTransformations)
                {
                    transformed = transformer(transformed);
                }

                return transformed;
            };
        }

        return transformer;
    }
}
