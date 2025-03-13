// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.WebApi.Controllers;

/// <summary>
/// Represents the controller for the time API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimeController(TimeProvider timeProvider) : ControllerBase
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    /// <returns>
    /// The current date and time.
    /// </returns>
    /// <remarks>
    /// The current date and time is returned in Coordinated Universal Time (UTC).
    /// </remarks>
    /// <example>
    /// <c>{"utcNow":"2025-03-13T10:18:37.8147935+00:00"}</c>
    /// </example>
    [HttpGet]
    public ActionResult<TimeModel> Now()
        => Ok(new TimeModel { UtcNow = timeProvider.GetUtcNow() });
}
