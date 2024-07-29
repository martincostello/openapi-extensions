// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.OpenApi;

public sealed class TestFixture(
    Action<IServiceCollection> configureServices,
    Action<IEndpointRouteBuilder> configureEndpoints,
    ITestOutputHelper outputHelper) : WebApplicationFactory<Program>()
{
    public async Task<string> GetOpenApiDocumentAsync()
    {
        using var client = CreateDefaultClient();
        return await client.GetStringAsync("/openapi/v1.json");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging((builder) =>
        {
            builder.ClearProviders()
                   .AddXUnit(outputHelper)
                   .SetMinimumLevel(LogLevel.Information);
        });

        builder.ConfigureServices((services) =>
        {
            services.AddSingleton<IStartupFilter>(ConfigureEndpointsFilter.Create(configureEndpoints));
            configureServices(services);
        });
    }

    private sealed class ConfigureEndpointsFilter(Action<IEndpointRouteBuilder> configure) : IStartupFilter
    {
#pragma warning disable CA1859
        public static IStartupFilter Create(Action<IEndpointRouteBuilder> configure)
            => new ConfigureEndpointsFilter(configure);
#pragma warning restore CA1859

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
