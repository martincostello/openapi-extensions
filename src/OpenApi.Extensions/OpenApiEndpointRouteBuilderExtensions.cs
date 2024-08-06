// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

//// Based on https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Extensions/OpenApiEndpointRouteBuilderExtensions.cs

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace MartinCostello.OpenApi;

/// <summary>
/// OpenAPI-related extension methods for <see cref="IEndpointRouteBuilder"/>. This class cannot be inherited.
/// </summary>
public static class OpenApiEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Registers an endpoint onto the current application for resolving the OpenAPI document
    /// associated with the current application that is returned as YAML.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route to register the endpoint on. Must include the <c>documentName</c> route parameter.</param>
    /// <returns>An <see cref="IEndpointRouteBuilder"/> that can be used to further customize the endpoint.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="endpoints"/> is <see langword="null"/>.
    /// </exception>
    public static IEndpointConventionBuilder MapOpenApiYaml(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern = OpenApiConstants.DefaultOpenApiRouteAsYaml)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var documentService = new OpenApiDocumentService();
        var options = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>();

        return endpoints.MapGet(
            pattern,
            async (HttpContext context, string documentName = OpenApiConstants.DefaultDocumentName) =>
            {
                var document = await documentService.TryGetOpenApiDocumentAsync(
                    context.RequestServices,
                    documentName,
                    context.RequestAborted);

                if (document is null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    await context.Response.WriteAsync($"No OpenAPI document with the name '{documentName}' was found.");
                }
                else
                {
                    var documentOptions = options.Get(documentName);
                    await WriteDocumentAsync(document, documentOptions.OpenApiVersion, context);
                }
            }).ExcludeFromDescription();
    }

    private static async Task WriteDocumentAsync(
        OpenApiDocument document,
        OpenApiSpecVersion version,
        HttpContext httpContext)
    {
        using var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream);
        var yamlWriter = new ScrubbingOpenApiYamlWriter(streamWriter);

        document.Serialize(yamlWriter, version);

        // See https://www.rfc-editor.org/rfc/rfc9512.html
        httpContext.Response.ContentType = "application/yaml";

        await httpContext.Response.BodyWriter.WriteAsync(stream.ToArray(), httpContext.RequestAborted);
        await httpContext.Response.BodyWriter.FlushAsync(httpContext.RequestAborted);
    }

    private sealed class OpenApiDocumentService
    {
        // See https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Services/OpenApiDocumentService.cs#L55
        private const string OpenApiAssemblyName = "Microsoft.AspNetCore.OpenApi";
        private const string OpenApiDocumentServiceName = "Microsoft.AspNetCore.OpenApi.OpenApiDocumentService";
        private const string OpenApiDocumentServiceFullyQualifiedName = $"{OpenApiDocumentServiceName}, {OpenApiAssemblyName}";
        private const string GetOpenApiDocumentAsyncMethodName = "GetOpenApiDocumentAsync";

        private readonly Type _documentService;
        private readonly MethodInfo _getDocument;

        public OpenApiDocumentService()
        {
            (_documentService, _getDocument) = GetOpenApiDocumentAsyncMethod();
        }

        public async Task<OpenApiDocument?> TryGetOpenApiDocumentAsync(
            IServiceProvider serviceProvider,
            string documentName,
            CancellationToken cancellationToken)
        {
            if (!TryGetServiceInstance(serviceProvider, documentName, out var instance))
            {
                return null;
            }

            return await GetOpenApiDocumentAsync(instance, cancellationToken);
        }

        [DynamicDependency(GetOpenApiDocumentAsyncMethodName, OpenApiDocumentServiceName, OpenApiAssemblyName)]
        private static (Type Type, MethodInfo Method) GetOpenApiDocumentAsyncMethod()
        {
            var type = Type.GetType(OpenApiDocumentServiceFullyQualifiedName, throwOnError: true);
            Debug.Assert(type is not null, $"Could not resolve type \"{OpenApiDocumentServiceFullyQualifiedName}\"");

            var methodInfo = type.GetMethod(
                GetOpenApiDocumentAsyncMethodName,
                BindingFlags.Instance | BindingFlags.Public,
                [typeof(CancellationToken)]);

            Debug.Assert(methodInfo is not null, $"Could not resolve method {GetOpenApiDocumentAsyncMethodName} from type {OpenApiDocumentServiceName}.");
            return (type, methodInfo!);
        }

        private bool TryGetServiceInstance(
            IServiceProvider serviceProvider,
            string documentName,
            [NotNullWhen(true)] out object? instance)
        {
            // TODO Simplify if API proposal if ever implemented: https://github.com/dotnet/runtime/issues/105828
            if (serviceProvider is IKeyedServiceProvider keyedServiceProvider)
            {
                instance = keyedServiceProvider.GetKeyedService(_documentService, documentName);
                return instance is not null;
            }

            try
            {
                // There isn't an AoT-friendly IServiceProvider.GetKeyedService(Type serviceType, object? serviceKey) method
                // so use the GetRequiredKeyedService() method instance and catch the exception if it is not found.
                instance = serviceProvider.GetRequiredKeyedService(_documentService, documentName);
                return true;
            }
            catch (InvalidOperationException)
            {
                instance = null;
                return false;
            }
        }

        private async Task<OpenApiDocument> GetOpenApiDocumentAsync(object instance, CancellationToken cancellationToken)
        {
            var result = (Task<OpenApiDocument>)_getDocument.Invoke(instance, [cancellationToken])!;
            return await result;
        }
    }

    //// TODO Remove when RC.1 is available
    //// See https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Writers/ScrubbingOpenApiJsonWriter.cs

    private sealed class ScrubbingOpenApiYamlWriter(TextWriter textWriter) : OpenApiYamlWriter(textWriter)
    {
        public override void WritePropertyName(string name)
        {
            if (name is OpenApiConstants.SchemaId or OpenApiConstants.DescriptionId)
            {
                return;
            }

            base.WritePropertyName(name);
        }
    }
}
