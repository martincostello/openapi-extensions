// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace Models.A;

/// <summary>
/// Represents an animal.
/// </summary>
[OpenApiExample<IAnimal, AnimalExampleProvider>]
public interface IAnimal
{
    /// <summary>
    /// Gets or sets the name of the animal.
    /// </summary>
    string? Name { get; set; }
}
