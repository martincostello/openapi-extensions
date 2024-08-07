// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApp;

namespace MartinCostello.OpenApi;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
[ShortRunJob]
public class OpenApiBenchmarks : IAsyncDisposable
{
    private WebApplication? _app;
    private HttpClient? _client;
    private bool _disposed;

    public OpenApiBenchmarks()
    {
        var builder = WebApplication.CreateBuilder([$"--contentRoot={GetContentRoot()}"]);

        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("https://127.0.0.1:0");

        builder.AddTodoApp();

        _app = builder.Build();
        _app.UseTodoApp();
    }

    [GlobalSetup]
    public async Task StartServer()
    {
        if (_app is { } app)
        {
            await app.StartAsync();

            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            var baseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };

#pragma warning disable CA5400
            _client = new(handler, disposeHandler: true) { BaseAddress = baseAddress };
#pragma warning restore CA5400
        }
    }

    [GlobalCleanup]
    public async Task StopServer()
    {
        if (_app is { } app)
        {
            await app.StopAsync();
            _app = null;
        }
    }

    [Benchmark]
    public async Task<string> GetOpenApiDocumentJson()
        => await _client!.GetStringAsync("/openapi/v1.json");

    [Benchmark]
    public async Task<string> GetOpenApiDocumentYaml()
        => await _client!.GetStringAsync("/openapi/v1.yaml");

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            _client?.Dispose();

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
