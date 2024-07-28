// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

/// <summary>
/// Represents a greeting.
/// </summary>
[OpenApiExample<Greeting>]
public sealed class Greeting : IExampleProvider<Greeting>
{
    /// <summary>
    /// Gets or sets the text of the greeting.
    /// </summary>
    public string? Text { get; set; }

    /// <inheritdoc/>
    public static Greeting GenerateExample()
    {
        return new() { Text = "Hello, World!" };
    }
}
