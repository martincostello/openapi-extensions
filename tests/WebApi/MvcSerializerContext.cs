// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using MartinCostello.WebApi.Models;

namespace MartinCostello.WebApi;

[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(TimeModel))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.Strict,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
public sealed partial class MvcSerializerContext : JsonSerializerContext;
