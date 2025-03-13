// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi.Services;

/// <summary>
/// Represents a service for work with descriptions.
/// </summary>
internal interface IDescriptionService
{
    /// <summary>
    /// Gets description for member.
    /// </summary>
    /// <param name="memberName">Member full name.</param>
    /// <param name="paramName">Parameter name of member. Optional.</param>
    /// <param name="section">Section to get. Default is "summary".</param>
    /// <returns>Description for given member.</returns>
    string? GetDescription(string memberName, string? paramName = null, string? section = "summary");
}
