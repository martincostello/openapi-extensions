// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp.Models;

/// <summary>
/// Represents the model for searching for Todo items.
/// </summary>
[OpenApiExample<TodoItemFilterModel>]
public sealed class TodoItemFilterModel : IExampleProvider<TodoItemFilterModel>
{
    /// <summary>
    /// Gets or sets the text of the filter.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to search completed Todo items.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <inheritdoc/>
    public static TodoItemFilterModel GenerateExample() => new()
    {
        Text = "Buy eggs 🥚",
        IsCompleted = false,
    };
}
