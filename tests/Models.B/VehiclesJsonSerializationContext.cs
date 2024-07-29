// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Models.B;

/// <summary>
/// The <see cref="JsonSerializerContext"/> to use for vehicles.
/// </summary>
[JsonSerializable(typeof(Car))]
[JsonSerializable(typeof(Motorcycle))]
[JsonSerializable(typeof(Vehicle))]
public sealed partial class VehiclesJsonSerializationContext : JsonSerializerContext;
