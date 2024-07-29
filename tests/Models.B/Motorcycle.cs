// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace Models.B;

/// <summary>
/// Represents a motorcycle.
/// </summary>
/// <param name="Manufacturer">The name of the manufacturer.</param>
/// <param name="HasSidecar">Whether the motorcycle has a sidecar.</param>
public record Motorcycle(string Manufacturer, bool HasSidecar = false) : Vehicle(2, Manufacturer);
