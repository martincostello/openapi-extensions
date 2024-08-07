// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.OpenApi;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmarks = new OpenApiBenchmarks();
    await benchmarks.StartServer();

    try
    {
        _ = await benchmarks.GetOpenApiDocumentJson();
        _ = await benchmarks.GetOpenApiDocumentYaml();
    }
    finally
    {
        await benchmarks.StopServer();
    }
}
else
{
    BenchmarkRunner.Run<OpenApiBenchmarks>(args: args);
}
