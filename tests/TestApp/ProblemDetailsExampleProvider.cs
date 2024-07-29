// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.OpenApi;

/// <summary>
/// A class representing an example provider for <see cref="ProblemDetails"/>.
/// </summary>
public sealed class ProblemDetailsExampleProvider : IExampleProvider<ProblemDetails>
{
    /// <inheritdoc/>
    public static ProblemDetails GenerateExample()
    {
        return new()
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An internal error occurred.",
            Instance = "/hello",
        };
    }
}
