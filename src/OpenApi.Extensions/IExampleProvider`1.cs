// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

/// <summary>
/// Defines a method for obtaining examples for use in OpenAPI documentation.
/// </summary>
/// <typeparam name="T">The type of the example.</typeparam>
public interface IExampleProvider<T>
{
    /// <summary>
    /// Generates the example to use.
    /// </summary>
    /// <returns>
    /// A <typeparamref name="T"/> that should be used as the example.
    /// </returns>
    static abstract T GenerateExample();
}
