// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.OpenApi;

public class DocumentTests(ITestOutputHelper outputHelper)
{
    private static readonly VerifySettings Settings = CreateSettings();

    [Fact]
    public async Task Schema_Is_Correct()
    {
        // Arrange
        using var fixture = new TestFixture(
            (services) =>
            {
                services.AddHttpContextAccessor();
                services.AddOpenApiExtensions((options) =>
                {
                    options.AddExamples = true;
                    options.AddServerUrls = true;
                    options.SerializationContext = AppJsonSerializationContext.Default;

                    options.AddExample<ProblemDetails, ProblemDetailsExampleProvider>();
                    options.AddXmlComments<Greeting>();
                });
            },
            (endpoints) => { },
            outputHelper);

        // Act
        var actual = await fixture.GetOpenApiDocumentAsync();

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
