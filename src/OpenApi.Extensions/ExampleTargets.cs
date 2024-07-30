// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.OpenApi;

/// <summary>
/// A class that defines the valid attribute targets for example attributes.
/// </summary>
internal static class ExampleTargets
{
    /// <summary>
    /// The valid attribute targets for example attributes.
    /// </summary>
    public const AttributeTargets ValidTargets =
        AttributeTargets.Class |
        AttributeTargets.Enum |
        AttributeTargets.Interface |
        AttributeTargets.Method |
        AttributeTargets.Parameter |
        AttributeTargets.ReturnValue |
        AttributeTargets.Struct;
}
