// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that adds examples to the OpenAPI operations and schemas. This class cannot be inherited.
/// </summary>
/// <param name="examplesMetadata">Any explicitly configured example metadata.</param>
/// <param name="context">The <see cref="JsonSerializerContext"/> to use to generate the examples.</param>
internal sealed class AddExamplesTransformer(
    ICollection<IOpenApiExampleMetadata> examplesMetadata,
    JsonSerializerContext context) :
    IOpenApiOperationTransformer,
    IOpenApiSchemaTransformer
{
    private readonly ExamplesCache _cache = new(examplesMetadata);
    private readonly JsonSerializerContext _context = context;

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        Process(operation, context.Description);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Process(schema, context.JsonTypeInfo.Type);

        return Task.CompletedTask;
    }

    private static MethodInfo? TryGetMethodInfo(ApiDescription description)
        => description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();

    private void Process(OpenApiOperation operation, ApiDescription description)
    {
        // Get all the examples that may apply to the operation through attributes
        // configured globally, on an API group, or on a specific endpoint.
        var examples = description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        if (_cache.GetMetadata(description) is { Length: > 0 } metadata)
        {
            examples = [.. examples, .. metadata];
        }

        if (operation.Parameters is { Count: > 0 } parameters)
        {
            TryAddParameterExamples(parameters, description, examples);
        }

        if (operation.RequestBody is { } body)
        {
            TryAddRequestExamples(body, description, examples);
        }

        TryAddResponseExamples(operation.Responses, description, examples);
    }

    private void Process(OpenApiSchema schema, Type type)
    {
        if (schema.Example is null &&
            _cache.TryGetMetadata(type, includeOptions: true) is { } metadata)
        {
            schema.Example = metadata.GenerateExample(_context);
        }
    }

    private void TryAddParameterExamples(
        IList<OpenApiParameter> parameters,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        // Find the method associated with the operation and get its arguments
        if (TryGetMethodInfo(description) is not { } method)
        {
            return;
        }

        foreach (var argument in method.GetParameters())
        {
            if (TryGetMetadata(argument, examples) is { } metadata)
            {
                // Find the parameter that corresponds to the argument and set its example
                var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                if (parameter is not null)
                {
                    parameter.Example ??= metadata.GenerateExample(_context);
                }
            }
        }
    }

    private void TryAddRequestExamples(
        OpenApiRequestBody body,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        if (TryGetMethodInfo(description) is not { } method)
        {
            return;
        }

        foreach (var mediaType in body.Content.Values)
        {
            var bodyParameter = description.ParameterDescriptions.Single((p) => p.Source == BindingSource.Body);
            var argument = method.GetParameters().Single((p) => p.Name == bodyParameter.Name);

            if (TryGetMetadata(argument, bodyParameter, examples) is { } metadata)
            {
                mediaType.Example ??= metadata.GenerateExample(_context);
            }
        }
    }

    private void TryAddResponseExamples(
        OpenApiResponses responses,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        foreach (var schemaResponse in description.SupportedResponseTypes)
        {
            schemaResponse.Type ??= schemaResponse.ModelMetadata?.ModelType;

            IOpenApiExampleMetadata? metadata = null;

            if (schemaResponse.Type is { } type)
            {
                // Try to find example metadata in the following order of precedence:
                // 1. From the endpoint metadata
                // 2. From the response's type
                // 3. From examples configured in the options
                metadata =
                    examples.FirstOrDefault((p) => p.ExampleType == type) ??
                    _cache.TryGetMetadata(type, includeOptions: true);
            }

            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                if (responses.TryGetValue(schemaResponse.StatusCode.ToString(CultureInfo.InvariantCulture), out var response) &&
                    response.Content.TryGetValue(responseFormat.MediaType, out var mediaType))
                {
                    mediaType.Example ??= metadata?.GenerateExample(_context);
                }
            }
        }
    }

    private IOpenApiExampleMetadata? TryGetMetadata(
        ParameterInfo argument,
        ApiParameterDescription bodyParameter,
        IList<IOpenApiExampleMetadata> examples)
    {
        // Try to find example metadata in the following order of precedence:
        // 1. From parameter attributes
        // 2. From the parameter's type
        // 3. From the endpoint metadata
        // 4. From examples configured in the options
        return
            _cache.TryGetMetadata(argument) ??
            _cache.TryGetMetadata(bodyParameter.Type, includeOptions: false) ??
            examples.FirstOrDefault((p) => p.ExampleType == bodyParameter.Type) ??
            _cache.TryGetMetadata(bodyParameter.Type, includeOptions: true);
    }

    private IOpenApiExampleMetadata? TryGetMetadata(
        ParameterInfo argument,
        IList<IOpenApiExampleMetadata> examples)
    {
        // Try to find example metadata in the following order of precedence:
        // 1. From parameter attributes
        // 2. From the parameter's type
        // 3. From the endpoint metadata
        // 4. From examples configured in the options
        return
            _cache.TryGetMetadata(argument) ??
            _cache.TryGetMetadata(argument.ParameterType, includeOptions: false) ??
            examples.FirstOrDefault((p) => p.ExampleType == argument.ParameterType) ??
            _cache.TryGetMetadata(argument.ParameterType, includeOptions: true);
    }

    private sealed class ExamplesCache(ICollection<IOpenApiExampleMetadata> examplesMetadata)
    {
        private readonly FrozenDictionary<Type, IOpenApiExampleMetadata> _typeMetadata = examplesMetadata.ToFrozenDictionary((p) => p.ExampleType, (v) => v);
        private readonly ConcurrentDictionary<MethodInfo, IOpenApiExampleMetadata[]> _methodMetadataCache = [];
        private readonly ConcurrentDictionary<ParameterInfo, IOpenApiExampleMetadata?> _parameterMetadataCache = [];
        private readonly ConcurrentDictionary<(Type Type, bool IncludeOptions), IOpenApiExampleMetadata?> _typeMetadataCache = [];

        public IOpenApiExampleMetadata[] GetMetadata(ApiDescription description)
        {
            if (TryGetMethodInfo(description) is not { } method)
            {
                return [];
            }

            return _methodMetadataCache.GetOrAdd(method, static (p) => [.. p.GetExampleMetadata()]);
        }

        public IOpenApiExampleMetadata? TryGetMetadata(Type type, bool includeOptions)
        {
            return _typeMetadataCache.GetOrAdd((type, includeOptions), (key) =>
            {
                if (key.Type.GetExampleMetadata() is { } metadata)
                {
                    return metadata;
                }

                if (!key.IncludeOptions)
                {
                    return null;
                }

                return _typeMetadata.TryGetValue(key.Type, out metadata) ? metadata : null;
            });
        }

        public IOpenApiExampleMetadata? TryGetMetadata(ParameterInfo parameter)
        {
            return _parameterMetadataCache.GetOrAdd(
                parameter,
                static (key) => key.GetExampleMetadata().FirstOrDefault((p) => p.ExampleType == key.ParameterType));
        }
    }
}
