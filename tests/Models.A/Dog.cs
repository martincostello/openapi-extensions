// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
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

    /// <summary>
    /// Gets the age of the dog, if known.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? Age { get; init; }

    /// <inheritdoc/>
    public static Dog GenerateExample() => new()
    {
        Breed = "Greyhound",
        Name = "Santa's Little Helper",
    };
}
