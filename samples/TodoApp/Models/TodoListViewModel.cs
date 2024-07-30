// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp.Models;

/// <summary>
/// Represents a collection of Todo items.
/// </summary>
[OpenApiExample<TodoListViewModel>]
public class TodoListViewModel : IExampleProvider<TodoListViewModel>
{
    /// <summary>
    /// Gets or sets the Todo item(s).
    /// </summary>
    public ICollection<TodoItemModel> Items { get; set; } = [];

    /// <inheritdoc/>
    public static TodoListViewModel GenerateExample()
    {
        return new()
        {
            Items = [TodoItemModel.GenerateExample()],
        };
    }
}
