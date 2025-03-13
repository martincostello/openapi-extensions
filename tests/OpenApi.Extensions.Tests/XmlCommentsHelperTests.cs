// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Numerics;

namespace MartinCostello.OpenApi;

public static class XmlCommentsHelperTests
{
    [Theory]
    [InlineData(typeof(SomeClass), nameof(SomeClass.SomeMethod), "M:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeClass.SomeMethod")]
    [InlineData(typeof(SomeClass), nameof(SomeClass.Add), "M:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeClass.Add(System.Int32,System.Int32)")]
    [InlineData(typeof(SomeClass), nameof(SomeClass.AddGeneric), "M:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeClass.AddGeneric``1(``0,``0)")]
    [InlineData(typeof(SomeClass), nameof(SomeClass.AddMultipleGenerics), "M:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeClass.AddMultipleGenerics``3(``0,``1)")]
    [InlineData(typeof(SomeClass), nameof(SomeClass.ConcatenateArrays), "M:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeClass.ConcatenateArrays(System.Int32[],System.Int32[])")]
    [InlineData(typeof(SomeClass), nameof(SomeClass.ConcatenateLists), "M:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeClass.ConcatenateLists(System.Collections.Generic.List{System.Int32},System.Collections.Generic.List{System.Int32})")]
    public static void GetMemberName_For_Method(Type type, string name, string expected)
    {
        // Arrange
        var method = type.GetMethod(name)!;

        // Act
        var actual = XmlCommentsHelper.GetMemberName(method);

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(typeof(SomeStruct), nameof(SomeStruct.Field), "F:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeStruct.Field")]
    [InlineData(typeof(SomeGenericClass<int>), nameof(SomeGenericClass<int>.Field), "F:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeGenericClass`1.Field")]
    public static void GetMemberName_For_Field(Type type, string name, string expected)
    {
        // Arrange
        var method = type.GetField(name)!;

        // Act
        var actual = XmlCommentsHelper.GetMemberName(method);

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(typeof(SomeStruct), nameof(SomeStruct.ArrayProperty), "P:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeStruct.ArrayProperty")]
    [InlineData(typeof(SomeStruct), nameof(SomeStruct.GenericProperty), "P:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeStruct.GenericProperty")]
    [InlineData(typeof(SomeStruct), nameof(SomeStruct.Property), "P:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeStruct.Property")]
    [InlineData(typeof(SomeGenericClass<int>), nameof(SomeGenericClass<int>.ArrayProperty), "P:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeGenericClass`1.ArrayProperty")]
    [InlineData(typeof(SomeGenericClass<int>), nameof(SomeGenericClass<int>.GenericProperty), "P:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeGenericClass`1.GenericProperty")]
    [InlineData(typeof(SomeGenericClass<int>), nameof(SomeGenericClass<int>.Property), "P:MartinCostello.OpenApi.XmlCommentsHelperTests.SomeGenericClass`1.Property")]
    public static void GetMemberName_For_Property(Type type, string name, string expected)
    {
        // Arrange
        var method = type.GetProperty(name)!;

        // Act
        var actual = XmlCommentsHelper.GetMemberName(method);

        // Assert
        actual.ShouldBe(expected);
    }

    private sealed class SomeStruct
    {
#pragma warning disable SA1401
        /// <summary>
        /// A field.
        /// </summary>
        public int Field = 2;
#pragma warning restore SA1401

        /// <summary>
        /// Gets or sets a generic property.
        /// </summary>
        public List<int> GenericProperty { get; set; } = [];

        /// <summary>
        /// Gets or sets an array property.
        /// </summary>
        public List<int> ArrayProperty { get; set; } = [];

        /// <summary>
        /// Gets or sets a property.
        /// </summary>
        public int Property { get; set; }
    }

    private sealed class SomeGenericClass<T>(T value)
    {
#pragma warning disable SA1401
        /// <summary>
        /// A field.
        /// </summary>
        public T Field = value;
#pragma warning restore SA1401

        /// <summary>
        /// Gets or sets a generic property.
        /// </summary>
        public List<T> GenericProperty { get; set; } = [];

        /// <summary>
        /// Gets or sets an array property.
        /// </summary>
        public List<T> ArrayProperty { get; set; } = [];

        /// <summary>
        /// Gets or sets a property.
        /// </summary>
        public T Property { get; set; } = value;
    }

    /// <summary>
    /// A class with some methods to test with.
    /// </summary>
    private sealed class SomeClass
    {
        /// <summary>
        /// A method with no parameters.
        /// </summary>
        public static void SomeMethod()
        {
            // No-op
        }

        /// <summary>
        /// Adds two integers together.
        /// </summary>
        /// <param name="first">The first integer.</param>
        /// <param name="second">The second integer.</param>
        /// <returns>The sum.</returns>
        public static int Add(int first, int second) => first + second;

        /// <summary>
        /// Concatenates two arrays of integers together.
        /// </summary>
        /// <param name="first">The first array.</param>
        /// <param name="second">The second array.</param>
        /// <returns>The combined arrays.</returns>
        public static int[] ConcatenateArrays(int[] first, int[] second) => [.. first, .. second];

        /// <summary>
        /// Concatenates two lists of integers together.
        /// </summary>
        /// <param name="first">The first list.</param>
        /// <param name="second">The second list.</param>
        /// <returns>The combined arrays.</returns>
        public static List<int> ConcatenateLists(List<int> first, List<int> second) => [.. first, .. second];

        /// <summary>
        /// Adds two numbers together.
        /// </summary>
        /// <typeparam name="T">The type of the numbers.</typeparam>
        /// <param name="first">The first number.</param>
        /// <param name="second">The second number.</param>
        /// <returns>The sum.</returns>
        public static T AddGeneric<T>(T first, T second)
            where T : INumber<T>
            => first + second;

        /// <summary>
        /// Adds two numbers together.
        /// </summary>
        /// <typeparam name="TFirst">The type of the first number.</typeparam>
        /// <typeparam name="TSecond">The type of the second number.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="first">The first number.</param>
        /// <param name="second">The second number.</param>
        /// <returns>The sum.</returns>
        public static TResult AddMultipleGenerics<TFirst, TSecond, TResult>(TFirst first, TSecond second)
            where TFirst : INumber<TFirst>
            where TSecond : INumber<TSecond>
            where TResult : INumber<TResult>
            => TResult.CreateChecked<TFirst>(first) + TResult.CreateChecked<TSecond>(second);
    }
}
