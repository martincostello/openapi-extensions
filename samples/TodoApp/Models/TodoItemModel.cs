// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp.Models;

/// <summary>
/// Represents a Todo item.
/// </summary>
[OpenApiExample<TodoItemModel>]
public class TodoItemModel : IExampleProvider<TodoItemModel>
{
    /// <summary>
    /// Gets or sets the ID of the Todo item.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the text of the Todo item.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the Todo item has been completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time the Todo item was last updated.
    /// </summary>
    public string LastUpdated { get; set; } = default!;

    /// <inheritdoc/>
    public static TodoItemModel GenerateExample()
    {
        return new()
        {
            Id = "a03952ca-880e-4af7-9cfa-630be0feb4a5",
            IsCompleted = false,
            Text = "Buy eggs 🥚",
        };
    }
}
