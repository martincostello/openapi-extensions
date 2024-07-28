// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.OpenApi;

/// <summary>
/// The <see cref="JsonSerializerContext"/> to use for the application. This class cannot be inherited.
/// </summary>
[JsonSerializable(typeof(Greeting))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
public sealed partial class AppJsonSerializationContext : JsonSerializerContext;
