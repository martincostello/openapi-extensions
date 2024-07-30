// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace Models.A;

/// <summary>
/// A class representing a dog. Secret.
/// </summary>
[OpenApiExample<Dog>]
public class Dog : Animal, IExampleProvider<Dog>
{
    /// <summary>
    /// Gets or sets the breed of the dog.
    /// </summary>
    public string? Breed { get; set; }

    /// <inheritdoc/>
    public static Dog GenerateExample() => new()
    {
        Breed = "Greyhound",
        Name = "Santa's Little Helper",
    };
}
