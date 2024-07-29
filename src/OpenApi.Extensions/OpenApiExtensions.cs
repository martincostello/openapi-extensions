// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class containing extension methods to add extensions for <c>Microsoft.AspNetCore.OpenApi</c>. This class cannot be inherited.
/// </summary>
public static class OpenApiExtensions
{
    // See https://github.com/dotnet/aspnetcore/blob/06155c05af89c957de20d2c53cee0e37171b9a09/src/OpenApi/src/Services/OpenApiConstants.cs#L10C5-L10C54
    private const string DefaultDocumentName = "v1";

    /// <summary>
    /// Adds OpenAPI extensions related to the default document to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register OpenAPI extensions with.</param>
    /// <returns>
    /// The current <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddOpenApiExtensions(this IServiceCollection services)
        => services.AddOpenApiExtensions(DefaultDocumentName);

    /// <summary>
    /// Adds OpenAPI extensions related to the default document to the specified <see cref="IServiceCollection"/> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register OpenAPI extensions with.</param>
    /// <param name="configureOptions">A delegate used to configure the target <see cref="OpenApiExtensionsOptions"/>.</param>
    /// <returns>
    /// The current <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configureOptions"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddOpenApiExtensions(this IServiceCollection services, Action<OpenApiExtensionsOptions> configureOptions)
            => services.AddOpenApiExtensions(DefaultDocumentName, configureOptions);

    /// <summary>
    /// Adds OpenAPI extensions related to the default document to the specified <see cref="IServiceCollection"/> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register OpenAPI extensions with.</param>
    /// <param name="configureOptions">A delegate used to configure the target <see cref="OpenApiExtensionsOptions"/>.</param>
    /// <returns>
    /// The current <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configureOptions"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddOpenApiExtensions(this IServiceCollection services, Action<OpenApiOptions, OpenApiExtensionsOptions> configureOptions)
            => services.AddOpenApiExtensions(DefaultDocumentName, configureOptions);

    /// <summary>
    /// Adds OpenAPI extensions related to the given document name to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register OpenAPI extensions with.</param>
    /// <param name="documentName">The name of the OpenAPI document associated with extensions.</param>
    /// <returns>
    /// The current <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddOpenApiExtensions(this IServiceCollection services, string documentName)
        => services.AddOpenApiExtensions(documentName, static (_, _) => { });

    /// <summary>
    /// Adds OpenAPI extensions related to the given document name to the specified <see cref="IServiceCollection"/> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register OpenAPI extensions with.</param>
    /// <param name="documentName">The name of the OpenAPI document associated with extensions.</param>
    /// <param name="configureOptions">A delegate used to configure the target <see cref="OpenApiExtensionsOptions"/>.</param>
    /// <returns>
    /// The current <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configureOptions"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddOpenApiExtensions(
        this IServiceCollection services,
        string documentName,
        Action<OpenApiExtensionsOptions> configureOptions)
        => services.AddOpenApiExtensions(documentName, (_, options) => configureOptions(options));

    /// <summary>
    /// Adds OpenAPI extensions related to the given document name to the specified <see cref="IServiceCollection"/> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register OpenAPI extensions with.</param>
    /// <param name="documentName">The name of the OpenAPI document associated with extensions.</param>
    /// <param name="configureOptions">A delegate used to configure the target <see cref="OpenApiExtensionsOptions"/>.</param>
    /// <returns>
    /// The current <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configureOptions"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddOpenApiExtensions(
        this IServiceCollection services,
        string documentName,
        Action<OpenApiOptions, OpenApiExtensionsOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOpenApiExtensionsCore(documentName);

        services.AddOptions<OpenApiOptions>(documentName)
                .Configure<IOptions<OpenApiExtensionsOptions>>(ConfigureExtensions);

        return services;

        void ConfigureExtensions(OpenApiOptions options, IOptions<OpenApiExtensionsOptions> other)
        {
            var extensions = other.Value;
            configureOptions(options, extensions);

            // TODO Register as the instance
            var parameterDescriptions = new AddParameterDescriptionsTransformer();
            var responseDescriptions = new AddResponseDescriptionsTransformer();
            options.UseOperationTransformer(parameterDescriptions.TransformAsync);
            options.UseOperationTransformer(responseDescriptions.TransformAsync);

            if (extensions.AddServerUrls)
            {
                options.UseTransformer<AddServersTransformer>();
            }

            if (extensions.AddExamples)
            {
                if (extensions.SerializationContext is null)
                {
                    throw new InvalidOperationException($"No {nameof(JsonSerializerContext)} has been configured on the {nameof(OpenApiExtensionsOptions)} instance for the OpenAPI document \"{options.DocumentName}\".");
                }

                var examples = new AddExamplesTransformer(
                    extensions.ExamplesMetadata,
                    extensions.SerializationContext);

                // TODO Register as the instance
                options.UseOperationTransformer(examples.TransformAsync);
                options.UseSchemaTransformer(examples.TransformAsync);
            }

            if (extensions.XmlDocumentationAssemblies is { Count: > 0 } assemblies)
            {
                foreach (var assembly in assemblies)
                {
                    // TODO Register as the instance
                    var documentation = new AddXmlDocumentationTransformer(assembly);
                    options.UseSchemaTransformer(documentation.TransformAsync);
                }
            }

            if (extensions.GetDescriptionTransformer() is { } transformer)
            {
                // TODO Register as the instance
                var descriptions = new DescriptionsTransformer(transformer);
                options.UseOperationTransformer(descriptions.TransformAsync);
                options.UseSchemaTransformer(descriptions.TransformAsync);
            }
        }
    }

    private static IServiceCollection AddOpenApiExtensionsCore(this IServiceCollection services, string documentName)
    {
        services.AddKeyedSingleton<IOpenApiDocumentTransformer, AddServersTransformer>(documentName);
        services.AddOptions<OpenApiExtensionsOptions>(documentName);

        return services;
    }
}
