// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Models.A;

/// <summary>
/// The <see cref="JsonSerializerContext"/> to use for animals.
/// </summary>
[JsonSerializable(typeof(Animal))]
[JsonSerializable(typeof(Animal[]))]
[JsonSerializable(typeof(Cat))]
[JsonSerializable(typeof(Dog))]
public sealed partial class AnimalsJsonSerializationContext : JsonSerializerContext;
