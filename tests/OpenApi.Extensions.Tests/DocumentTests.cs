// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MartinCostello.OpenApi;

public class DocumentTests(ITestOutputHelper outputHelper)
{
    private static readonly VerifySettings Settings = CreateSettings();

    [Fact]
    public async Task Schema_Is_Correct()
    {
        // Arrange
        using var fixture = new WebApplicationFactory<Program>()
            .WithWebHostBuilder((builder) =>
            {
                builder.ConfigureLogging(
                    (logging) =>
                        logging.ClearProviders()
                               .AddXUnit(outputHelper)
                               .SetMinimumLevel(LogLevel.Information));

                builder.ConfigureServices((services) =>
                {
                    services.AddHttpContextAccessor();
                    services.AddOpenApiExtensions((options) =>
                    {
                        options.AddExamples = true;
                        options.AddXmlComments<Greeting>();
                        options.SerializationContext = AppJsonSerializationContext.Default;
                    });
                });
            });

        using var client = fixture.CreateDefaultClient();

        // Act
        var actual = await client.GetStringAsync("/openapi/v1.json");

        // Assert
        await Verifier.VerifyJson(actual, Settings);
    }

    private static VerifySettings CreateSettings()
    {
        var settings = new VerifySettings();

        settings.DontScrubDateTimes();
        settings.DontScrubGuids();

        return settings;
    }
}
