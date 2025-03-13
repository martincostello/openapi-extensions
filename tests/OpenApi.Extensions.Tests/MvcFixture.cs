// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MartinCostello.OpenApi;

public sealed class MvcFixture(ITestOutputHelper outputHelper) : WebApplicationFactory<TimeModel>()
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.ConfigureXUnitLogging(outputHelper);
}
