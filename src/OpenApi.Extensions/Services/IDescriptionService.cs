// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi.Services;

/// <summary>
/// Represents a service for working with descriptions.
/// </summary>
internal interface IDescriptionService
{
    /// <summary>
    /// Gets the description for a member.
    /// </summary>
    /// <param name="memberName">The member's full name.</param>
    /// <param name="paramName">The optional parameter of the member.</param>
    /// <param name="section">The default section to get the description from.</param>
    /// <returns>The description for the specified member, if found.</returns>
    string? GetDescription(string memberName, string? paramName = null, string? section = "summary");
}
