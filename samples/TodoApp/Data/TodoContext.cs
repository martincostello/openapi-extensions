// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;

namespace TodoApp.Data;

/// <summary>
/// Represents the data context for the Todo application.
/// </summary>
/// <param name="options">The data context options to use.</param>
public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the Todo items.
    /// </summary>
    public DbSet<TodoItem> Items { get; set; } = default!;
}
