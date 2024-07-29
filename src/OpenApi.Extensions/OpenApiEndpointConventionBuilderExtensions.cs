// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class containing methods for adding routing metadata to endpoint
/// instances for OpenAPI using <see cref="IEndpointConventionBuilder"/>.
/// This class cannot be inherited.
/// </summary>
public static class OpenApiEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds OpenAPI response metadata to <see cref="EndpointBuilder.Metadata"/> for all builders produced by builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    /// <param name="description">The description of the response.</param>
    /// <returns>
    /// The current <typeparamref name="TBuilder"/> instance for chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static TBuilder ProducesOpenApiResponse<TBuilder>(this TBuilder builder, int statusCode, string description)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(new OpenApiResponseAttribute(statusCode, description));
    }
}
