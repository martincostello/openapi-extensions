// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApp;

namespace MartinCostello.OpenApi;

internal sealed class TodoAppServer : IAsyncDisposable
{
    private WebApplication? _app;
    private Uri? _baseAddress;
    private bool _disposed;

    public TodoAppServer()
    {
        var builder = WebApplication.CreateBuilder([$"--contentRoot={GetContentRoot()}"]);

        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("https://127.0.0.1:0");

        builder.AddTodoApp();

        _app = builder.Build();
        _app.UseTodoApp();
    }

    public HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };

#pragma warning disable CA5400
        return new(handler, disposeHandler: true) { BaseAddress = _baseAddress };
#pragma warning restore CA5400
    }

    public async Task StartAsync()
    {
        if (_app is { } app)
        {
            await app.StartAsync();

            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            _baseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();
        }
    }

    public async Task StopAsync()
    {
        if (_app is { } app)
        {
            await app.StopAsync();
            _app = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            if (_app is not null)
            {
                await _app.DisposeAsync();
            }
        }

        _disposed = true;
    }

    private static string GetContentRoot()
    {
        string contentRoot = string.Empty;
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(typeof(OpenApiBenchmarks).Assembly.Location)!);

        do
        {
            string? solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, "TodoApp.sln").FirstOrDefault();

            if (solutionPath is not null)
            {
                contentRoot = Path.GetFullPath(Path.Combine(directoryInfo.FullName, "samples", "TodoApp"));
                break;
            }

            directoryInfo = directoryInfo.Parent;
        }
        while (directoryInfo is not null);

        return contentRoot;
    }
}
