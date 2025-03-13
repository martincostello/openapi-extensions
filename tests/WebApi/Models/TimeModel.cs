// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.WebApi.Models;

/// <summary>
/// Represents the current date and time.
/// </summary>
public class TimeModel
{
    /// <summary>
    /// Gets or sets the current date and time in UTC.
    /// </summary>
    public DateTimeOffset UtcNow { get; set; }
}
