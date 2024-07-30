// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.Data;

/// <summary>
/// A class representing a Todo item.
/// </summary>
public class TodoItem
{
    /// <summary>
    /// Gets or sets the ID of the Todo item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the text of the Todo item.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets the date and time the Todo item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time the Todo item was completed, if any.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
