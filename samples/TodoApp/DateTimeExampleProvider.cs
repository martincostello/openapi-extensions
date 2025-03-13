// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp;

/// <summary>
/// A class representing an example provider for <see cref="DateTime"/> values.
/// </summary>
public sealed class DateTimeExampleProvider : IExampleProvider<DateTime>
{
    /// <inheritdoc/>
    public static DateTime GenerateExample() => new(2025, 03, 05, 16, 43, 44, DateTimeKind.Utc);
}
