// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc.Testing;

namespace MartinCostello.OpenApi;

internal static class WebApplicationFactoryExtensions
{
    public static async Task<string> GetOpenApiDocumentAsync<T>(this WebApplicationFactory<T> fixture)
        where T : class
    {
        using var client = fixture.CreateDefaultClient();
        return await client.GetStringAsync("/openapi/v1.json");
    }
}
