// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace MartinCostello.OpenApi.Transformers;

/// <summary>
/// A class that server information to an OpenAPI document. This class cannot be inherited.
/// </summary>
/// <param name="accessor">The <see cref="IHttpContextAccessor"/> to use, if available.</param>
/// <param name="extensionsOptions"> The configured <see cref="OpenApiExtensionsOptions"/>.</param>
/// <param name="forwardedHeadersOptions">The configured <see cref="ForwardedHeadersOptions"/>, if any.</param>
internal sealed class AddServersTransformer(
    IHttpContextAccessor? accessor,
    IOptions<OpenApiExtensionsOptions> extensionsOptions,
    IOptions<ForwardedHeadersOptions>? forwardedHeadersOptions) : IOpenApiDocumentTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (GetServerUrl() is { } url)
        {
            document.Servers = [new() { Url = url }];
        }

        return Task.CompletedTask;
    }

    private string? GetServerUrl()
    {
        if (!extensionsOptions.Value.AddServerUrls)
        {
            return null;
        }

        if (accessor?.HttpContext?.Request is not { } request)
        {
            return extensionsOptions.Value.DefaultServerUrl;
        }

        if (forwardedHeadersOptions?.Value is not { } options)
        {
            return null;
        }

        var scheme = TryGetFirstHeader(options.ForwardedProtoHeaderName) ?? request.Scheme;
        var host = TryGetFirstHeader(options.ForwardedHostHeaderName) ?? request.Host.ToString();

        return new Uri($"{scheme}://{host}").ToString().TrimEnd('/');

        string? TryGetFirstHeader(string name)
            => request.Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
    }
}
