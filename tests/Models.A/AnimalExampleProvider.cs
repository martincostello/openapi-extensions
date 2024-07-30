// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace Models.A;

/// <summary>
/// A class representing an example provider for instances of <see cref="Animal"/> and <see cref="IAnimal"/>. This class cannot be inherited.
/// </summary>
public sealed class AnimalExampleProvider : IExampleProvider<Animal>, IExampleProvider<IAnimal>
{
    /// <inheritdoc/>
    static Animal IExampleProvider<Animal>.GenerateExample() => new()
    {
        Name = "Donald",
    };

    /// <inheritdoc/>
    static IAnimal IExampleProvider<IAnimal>.GenerateExample() => new Animal()
    {
        Name = "Daisy",
    };
}
