// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

#pragma warning disable CA1852 // TODO Enable with .NET 9 preview 7

/// <summary>
/// A class that adds examples to the OpenAPI operations and schemas. This class cannot be inherited.
/// </summary>
/// <param name="context">The <see cref="JsonSerializerContext"/> to use to generate the examples.</param>
internal class AddExamplesTransformer(JsonSerializerContext context)
{
    //// TODO Implement IOpenApiOperationTransformer and IOpenApiSchemaTransformer
    //// TODO Make the class sealed
    //// TODO Remove virtual modifiers

    private readonly JsonSerializerContext _context = context;

    public virtual Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        Process(operation, context.Description);

        return Task.CompletedTask;
    }

    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // TODO Use context.JsonTypeInfo.Type with .NET 9 preview 7
        Type? type = null; // context.JsonTypeInfo.Type;

#pragma warning disable CA1508
        if (type is not null)
#pragma warning restore CA1508
        {
            Process(schema, type);
        }

        return Task.CompletedTask;
    }

    private void Process(OpenApiOperation operation, ApiDescription description)
    {
        // Get all the examples that may apply to the operation through attributes
        // configured globally, on an API group, or on a specific endpoint.
        var examples = description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

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
            type.GetExampleMetadata().FirstOrDefault() is { } metadata)
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
        var arguments = description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .FirstOrDefault()?
            .GetParameters()
            .ToArray();

        if (arguments is { Length: > 0 })
        {
            foreach (var argument in arguments)
            {
                // Find the example for the argument either as a parameter attribute,
                // an attribute on the parameter's type, or metadata from the endpoint.
                var metadata =
                    argument.GetExampleMetadata().FirstOrDefault((p) => p.ExampleType == argument.ParameterType) ??
                    argument.ParameterType.GetExampleMetadata().FirstOrDefault((p) => p.ExampleType == argument.ParameterType) ??
                    examples.FirstOrDefault((p) => p.ExampleType == argument.ParameterType);

                if (metadata?.GenerateExample(_context) is { } value)
                {
                    // Find the parameter that corresponds to the argument and set its example
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Example ??= value;
                    }
                }
            }
        }
    }

    private void TryAddRequestExamples(
        OpenApiRequestBody body,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        if (!body.Content.TryGetValue("application/json", out var mediaType) || mediaType.Example is not null)
        {
            return;
        }

        var bodyParameter = description.ParameterDescriptions.Single((p) => p.Source == BindingSource.Body);

        var metadata = description.ParameterDescriptions
            .Single((p) => p.Source == BindingSource.Body)
            .Type
            .GetExampleMetadata()
            .FirstOrDefault();

        metadata ??= examples.FirstOrDefault((p) => p.ExampleType == bodyParameter.Type);

        if (metadata is not null)
        {
            mediaType.Example ??= metadata.GenerateExample(_context);
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

            var metadata = schemaResponse.Type?
                .GetExampleMetadata()
                .FirstOrDefault((p) => p.ExampleType == schemaResponse.Type);

            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                if (responses.TryGetValue(schemaResponse.StatusCode.ToString(CultureInfo.InvariantCulture), out var response) &&
                    response.Content.TryGetValue(responseFormat.MediaType, out var mediaType))
                {
                    mediaType.Example ??= (metadata ?? examples.SingleOrDefault((p) => p.ExampleType == schemaResponse.Type))?.GenerateExample(_context);
                }
            }
        }
    }
}
