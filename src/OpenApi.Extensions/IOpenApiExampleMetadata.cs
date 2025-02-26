// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

/// <summary>
/// Defines metadata for an OpenAPI example.
/// </summary>
public partial interface IOpenApiExampleMetadata
{
    /// <summary>
    /// Gets the type associated with the example.
    /// </summary>
    Type ExampleType { get; }
}
