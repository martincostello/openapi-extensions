// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

public class ControllerTests(ITestOutputHelper outputHelper) : DocumentTests(outputHelper)
{
    [Fact]
    public async Task Schema_Is_Correct_For_Web_Api()
    {
        // Arrange
        using var fixture = new MvcFixture(OutputHelper);

        // Act
        var actual = await fixture.GetOpenApiDocumentAsync();

        // Assert
        await VerifyJson(actual, Settings).UniqueForTargetFrameworkAndVersion();
    }
}
