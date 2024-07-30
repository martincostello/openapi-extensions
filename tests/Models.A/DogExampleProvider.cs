// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace Models.A;

/// <summary>
/// A class representing an example provider for instances of <see cref="Dog"/>. This class cannot be inherited.
/// </summary>
public sealed class DogExampleProvider : IExampleProvider<Dog>
{
    /// <inheritdoc/>
    public static Dog GenerateExample() => new()
    {
        Breed = "Golden Retriever",
        Name = "Fido",
    };
}
