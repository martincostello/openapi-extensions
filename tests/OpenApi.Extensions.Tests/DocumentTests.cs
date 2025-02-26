// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.OpenApi;

public abstract class DocumentTests(ITestOutputHelper outputHelper)
{
    internal static VerifySettings Settings { get; } = CreateSettings();

    protected ITestOutputHelper OutputHelper { get; } = outputHelper;

    protected async Task VerifyOpenApiDocumentAsync(
        Action<IServiceCollection> configureServices,
        Action<IEndpointRouteBuilder> configureEndpoints,
        VerifySettings? settings = null)
    {
        // Arrange
        using var fixture = new TestFixture(configureServices, configureEndpoints, OutputHelper);

        // Act
        var actual = await fixture.GetOpenApiDocumentAsync();

        // Assert
        await VerifyJson(actual, settings ?? Settings).UniqueForTargetFrameworkAndVersion();
    }

    private static VerifySettings CreateSettings()
    {
        var settings = new VerifySettings();

        settings.DontScrubDateTimes();
        settings.DontScrubGuids();

        return settings;
    }
}
