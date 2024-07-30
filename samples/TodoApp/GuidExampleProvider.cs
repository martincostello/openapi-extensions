// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp;

/// <summary>
/// A class representing an example provider for GUIDs.
/// </summary>
public sealed class GuidExampleProvider : IExampleProvider<Guid>
{
    private static readonly Guid Value = Guid.Parse("a03952ca-880e-4af7-9cfa-630be0feb4a5");

    /// <inheritdoc/>
    public static Guid GenerateExample() => Value;
}
