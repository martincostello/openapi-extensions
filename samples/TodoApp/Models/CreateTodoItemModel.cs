// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.OpenApi;

namespace TodoApp.Models;

/// <summary>
/// Represents the model for creating a new Todo item.
/// </summary>
[OpenApiExample<CreateTodoItemModel>]
public class CreateTodoItemModel : IExampleProvider<CreateTodoItemModel>
{
    /// <summary>
    /// Gets or sets the text of the Todo item.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <inheritdoc/>
    public static CreateTodoItemModel GenerateExample()
    {
        return new()
        {
            Text = "Buy eggs 🥚",
        };
    }
}
