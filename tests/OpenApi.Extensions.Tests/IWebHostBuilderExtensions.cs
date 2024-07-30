// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MartinCostello.OpenApi;

internal static class IWebHostBuilderExtensions
{
    public static IWebHostBuilder ConfigureXUnitLogging(this IWebHostBuilder builder, ITestOutputHelper outputHelper)
    {
        return builder.ConfigureLogging((builder) =>
        {
            builder.ClearProviders()
                   .AddXUnit(outputHelper)
                   .SetMinimumLevel(LogLevel.Information);
        });
    }
}
