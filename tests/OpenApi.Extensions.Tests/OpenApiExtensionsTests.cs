// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MartinCostello.OpenApi;

public static class OpenApiExtensionsTests
{
    [Fact]
    public static void AddOpenApiExtensions_Validates_Arguments()
    {
        // Arrange
        string documentName = "api";

        IServiceCollection services = null!;

        // Act and Assert
        Should.Throw<ArgumentNullException>(services.AddOpenApiExtensions).ParamName.ShouldBe("services");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(documentName)).ParamName.ShouldBe("services");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(documentName, ConfigureOneService)).ParamName.ShouldBe("services");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(documentName, ConfigureTwoServices)).ParamName.ShouldBe("services");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(ConfigureOneService)).ParamName.ShouldBe("services");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(ConfigureTwoServices)).ParamName.ShouldBe("services");

        // Arrange
        services = new ServiceCollection();
        Action<OpenApiExtensionsOptions> configureOne = null!;
        Action<OpenApiOptions, OpenApiExtensionsOptions> configureTwo = null!;

        // Act and Assert
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(configureOne)).ParamName.ShouldBe("configureOptions");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(configureTwo)).ParamName.ShouldBe("configureOptions");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(documentName, configureOne)).ParamName.ShouldBe("configureOptions");
        Should.Throw<ArgumentNullException>(() => services.AddOpenApiExtensions(documentName, configureTwo)).ParamName.ShouldBe("configureOptions");

        static void ConfigureOneService(OpenApiExtensionsOptions options)
            => options.ShouldNotBeNull();

        static void ConfigureTwoServices(OpenApiOptions first, OpenApiExtensionsOptions second)
        {
            first.ShouldNotBeNull();
            second.ShouldNotBeNull();
        }
    }

    [Fact]
    public static void AddOpenApiExtensions_Throws_If_Examples_Enabled_But_Not_JsonSerializerContext_Specified()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddOpenApi();
        services.AddOpenApiExtensions((options) =>
        {
            options.AddExamples = true;
            options.SerializationContext = null;
        });

        using var serviceProvider = services.BuildServiceProvider();
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>();

        // Act and Assert
        var exception = Should.Throw<InvalidOperationException>(() => monitor.Get("v1"));
        exception.Message.ShouldBe(@"No JsonSerializerContext has been configured on the OpenApiExtensionsOptions instance for the OpenAPI document ""v1"".");
    }
}
