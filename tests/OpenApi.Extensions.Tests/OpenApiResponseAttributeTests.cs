// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

public static class OpenApiResponseAttributeTests
{
    [Fact]
    public static void OpenApiResponseAttribute_Constructor_Initializes_Instance()
    {
        // Arrange
        var statusCode = 200;
        var description = "OK";

        // Act
        var actual = new OpenApiResponseAttribute(statusCode, description);

        // Assert
        actual.HttpStatusCode.ShouldBe(statusCode);
        actual.Description.ShouldBe(description);
    }
}
