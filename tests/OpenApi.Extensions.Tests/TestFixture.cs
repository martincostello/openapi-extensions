// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.OpenApi;

public sealed class TestFixture(
    Action<IServiceCollection> configureServices,
    Action<IEndpointRouteBuilder> configureEndpoints,
    ITestOutputHelper outputHelper) : WebApplicationFactory<Program>()
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureXUnitLogging(outputHelper);
        builder.ConfigureServices((services) =>
        {
            services.AddSingleton<IStartupFilter>(new ConfigureEndpointsFilter(configureEndpoints));
            configureServices(services);
        });
    }

    private sealed class ConfigureEndpointsFilter(Action<IEndpointRouteBuilder> configure) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return (builder) =>
            {
                next(builder);
                builder.UseEndpoints(configure);
            };
        }
    }
}
