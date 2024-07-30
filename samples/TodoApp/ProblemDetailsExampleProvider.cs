// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp;

/// <summary>
/// A class representing an example provider for <see cref="ProblemDetails"/>.
/// </summary>
public class ProblemDetailsExampleProvider : IExampleProvider<ProblemDetails>
{
    /// <inheritdoc/>
    public static ProblemDetails GenerateExample()
    {
        return new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "The specified value is invalid.",
        };
    }
}
