// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

/// <summary>
/// Represents an OpenAPI operation response. This class cannot be inherited.
/// </summary>
/// <param name="httpStatusCode">The HTTP status code for the response.</param>
/// <param name="description">The description of the response.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class OpenApiResponseAttribute(int httpStatusCode, string description) : Attribute
{
    /// <summary>
    /// Gets the HTTP status code for the response.
    /// </summary>
    public int HttpStatusCode { get; } = httpStatusCode;

    /// <summary>
    /// Gets the description of the response.
    /// </summary>
    public string Description { get; } = description;
}
