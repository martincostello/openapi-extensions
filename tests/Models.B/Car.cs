// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace Models.B;

/// <summary>
/// Represents a car.
/// </summary>
/// <param name="Type">The type of the car.</param>
/// <param name="Wheels">The number of wheels the vehicle has.</param>
/// <param name="Manufacturer">The name of the manufacturer.</param>
public record Car(CarType Type, int Wheels, string Manufacturer) : Vehicle(Wheels, Manufacturer);
