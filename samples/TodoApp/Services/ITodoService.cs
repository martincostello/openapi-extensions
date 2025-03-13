// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Models;

namespace TodoApp.Services;

/// <summary>
/// Represents a service for managing Todo items.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Adds a new Todo item.
    /// </summary>
    /// <param name="text">The text of the Todo item.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the ID of the new Todo item.
    /// </returns>
    Task<string> AddItemAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a Todo item as completed.
    /// </summary>
    /// <param name="itemId">The ID of the Todo item to complete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns a value indicating whether the Todo item was completed.
    /// </returns>
    Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a Todo item.
    /// </summary>
    /// <param name="itemId">The ID of the Todo item to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns a value indicating whether the Todo item was deleted.
    /// </returns>
    Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a Todo item by its ID.
    /// </summary>
    /// <param name="itemId">The ID of the Todo item.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the Todo item, if found; otherwise <see langword="null"/>.
    /// </returns>
    Task<TodoItemModel?> GetAsync(Guid itemId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of Todo items.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the list of Todo items.
    /// </returns>
    Task<TodoListViewModel> GetListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Finds a list of Todo items by filter.
    /// </summary>
    /// <param name="filter">The <see cref="TodoItemFilterModel"/> to use to search.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the list of found Todo items.
    /// </returns>
    Task<TodoListViewModel> FindAsync(TodoItemFilterModel filter, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of Todo items created after a given date and time.
    /// </summary>
    /// <param name="dateTime"><see cref="DateTime"/> to look for items created after.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the list of found Todo items.
    /// </returns>
    Task<TodoListViewModel> GetAfterDateAsync(DateTime dateTime, CancellationToken cancellationToken);
}
