// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;

namespace MartinCostello.OpenApi;

public static class OpenApiEndpointConventionBuilderExtensionsTests
{
    [Fact]
    public static void ProducesOpenApiResponse_Throws_If_Builder_Is_Null()
    {
        // Arrange
        IEndpointConventionBuilder builder = null!;

        // Act and Assert
        Should.Throw<ArgumentNullException>(() => builder.ProducesOpenApiResponse(200, "OK"));
    }
}
