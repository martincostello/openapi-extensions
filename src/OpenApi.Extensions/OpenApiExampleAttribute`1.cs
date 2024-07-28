// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

#pragma warning disable CA1813

/// <summary>
/// An attribute representing an example for an OpenAPI document.
/// </summary>
/// <typeparam name="T">The type of the example.</typeparam>
public class OpenApiExampleAttribute<T>() : OpenApiExampleAttribute<T, T>()
    where T : IExampleProvider<T>;
