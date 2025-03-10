// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp;

/// <summary>
/// A class representing an example provider for DateTime.
/// </summary>
public sealed class DateTimeExampleProvider : IExampleProvider<DateTime>
{
    private static readonly DateTime Value =
        DateTime.Parse("2025-03-05T16:43:44.5274103+05:00", CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public static DateTime GenerateExample() => Value;
}
