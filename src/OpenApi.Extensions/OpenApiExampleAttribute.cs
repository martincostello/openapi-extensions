// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

/// <summary>
/// An attribute representing a string example for an OpenAPI document. This class cannot be inherited.
/// </summary>
/// <param name="value">The example's value.</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class OpenApiExampleAttribute(string value) : OpenApiExampleAttribute<string, OpenApiExampleAttribute>, IExampleProvider<string>
{
    /// <summary>
    /// Gets the example value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc/>
    static string IExampleProvider<string>.GenerateExample() => "value";

    /// <inheritdoc />
    public override string GenerateExample() => Value;
}
