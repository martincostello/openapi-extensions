// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

internal static class OpenApiConstants
{
    // See https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Services/OpenApiConstants.cs#L10
    internal const string DefaultDocumentName = "v1";

    // See https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Services/OpenApiConstants.cs#L12
    internal const string DefaultOpenApiRouteAsYaml = "/openapi/{documentName}.yaml";

    // See https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Services/OpenApiConstants.cs#L13
    internal const string DescriptionId = "x-aspnetcore-id";

    // See https://github.com/dotnet/aspnetcore/blob/0fee04e2e1e507f0c993fa902d53abdd7c5dff65/src/OpenApi/src/Services/OpenApiConstants.cs#L14
    internal const string SchemaId = "x-schema-id";
}
