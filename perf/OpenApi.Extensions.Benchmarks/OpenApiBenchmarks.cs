// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.OpenApi;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
[ShortRunJob]
public class OpenApiBenchmarks : IAsyncDisposable
{
    private TodoAppServer? _app = new();
    private HttpClient? _client;
    private bool _disposed;

    [GlobalSetup]
    public async Task StartServer()
    {
        if (_app is { } app)
        {
            await app.StartAsync();
            _client = app.CreateHttpClient();
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

    [Benchmark(Baseline = true)]
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
            _client = null;

            if (_app is not null)
            {
                await _app.DisposeAsync();
                _app = null;
            }
        }

        _disposed = true;
    }
}
