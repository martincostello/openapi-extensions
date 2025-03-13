// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MartinCostello.OpenApi.Transformers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class containing extension methods to add extensions for <c>Microsoft.AspNetCore.OpenApi</c>. This class cannot be inherited.
/// </summary>
public static class OpenApiExtensions
{
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
        => services.AddOpenApiExtensions(OpenApiConstants.DefaultDocumentName);

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
            => services.AddOpenApiExtensions(OpenApiConstants.DefaultDocumentName, configureOptions);

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
            => services.AddOpenApiExtensions(OpenApiConstants.DefaultDocumentName, configureOptions);

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
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return services.AddOpenApiExtensions(documentName, (_, options) => configureOptions(options));
    }

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

        services.AddOptions<OpenApiExtensionsOptions>(documentName);
        services.AddKeyedSingleton<IOpenApiDocumentTransformer>(documentName, static (provider, key) =>
        {
            var extensionsOptions = provider.GetRequiredKeyedService<IOptions<OpenApiExtensionsOptions>>(key);
            var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
            var forwardedHeadersOptions = provider.GetService<IOptions<ForwardedHeadersOptions>>();

            return new AddServersTransformer(extensionsOptions, httpContextAccessor, forwardedHeadersOptions);
        });

        services.AddOptions<OpenApiOptions>(documentName)
                .Configure<IOptions<OpenApiExtensionsOptions>>(ConfigureExtensions);

        return services;

        void ConfigureExtensions(OpenApiOptions options, IOptions<OpenApiExtensionsOptions> other)
        {
            var extensions = other.Value;
            configureOptions(options, extensions);

            // Use singleton instances for better performance.
            // See https://github.com/dotnet/aspnetcore/issues/57211.
            options.AddOperationTransformer(new AddParameterDescriptionsTransformer());
            options.AddOperationTransformer(new AddResponseDescriptionsTransformer());

            if (extensions.AddServerUrls)
            {
                options.AddDocumentTransformer<AddServersTransformer>();
            }

            if (extensions.AddExamples)
            {
                if (extensions.SerializationContexts is not { Count: > 0 } contexts)
                {
                    throw new InvalidOperationException($"No {nameof(JsonSerializerContext)} has been configured on the {nameof(OpenApiExtensionsOptions)} instance for the OpenAPI document \"{options.DocumentName}\".");
                }

                var serializationContext =
                    contexts.Count is 1 ?
                    contexts[0] :
                    new ChainedJsonSerializerContext(contexts);

                var examples = new AddExamplesTransformer(
                    extensions.ExamplesMetadata,
                    serializationContext);

                options.AddOperationTransformer(examples);
                options.AddSchemaTransformer(examples);
            }

            if (extensions.XmlDocumentationAssemblies is { Count: > 0 } assemblies)
            {
                foreach (var assembly in assemblies)
                {
                    var descriptions = new XmlDescriptionService(assembly);

                    options.AddSchemaTransformer(new AddSchemaXmlDocumentationTransformer(assembly, descriptions));
                    options.AddOperationTransformer(new AddOperationXmlDocumentationTransformer(descriptions));
                }
            }

            if (extensions.GetDescriptionTransformer() is { } transformer)
            {
                var descriptions = new DescriptionsTransformer(transformer);
                options.AddOperationTransformer(descriptions);
                options.AddSchemaTransformer(descriptions);
            }
        }
    }

    private sealed class ChainedJsonSerializerContext(ICollection<JsonSerializerContext> chain) : JsonSerializerContext(null)
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected override JsonSerializerOptions? GeneratedSerializerOptions => throw new NotSupportedException();

        public override JsonTypeInfo? GetTypeInfo(Type type)
        {
            foreach (var context in chain)
            {
                if (context.GetTypeInfo(type) is { } typeInfo)
                {
                    return typeInfo;
                }
            }

            return null;
        }
    }
}
