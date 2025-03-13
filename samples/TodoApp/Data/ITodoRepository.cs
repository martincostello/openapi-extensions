// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.Data;

/// <summary>
/// Represents a repository of Todo items.
/// </summary>
public interface ITodoRepository
{
    /// <summary>
    /// Adds a new Todo item.
    /// </summary>
    /// <param name="text">The text of the Todo item.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the ID of the new Todo item.
    /// </returns>
    Task<TodoItem> AddItemAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a Todo item as completed.
    /// </summary>
    /// <param name="itemId">The ID of the Todo item to complete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns a value indicating whether the Todo item was completed.
    /// </returns>
    Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Todo item.
    /// </summary>
    /// <param name="itemId">The ID of the Todo item to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns a value indicating whether the Todo item was deleted.
    /// </returns>
    Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a Todo item by its ID.
    /// </summary>
    /// <param name="itemId">The ID of the Todo item.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the Todo item, if found; otherwise <see langword="null"/>.
    /// </returns>
    Task<TodoItem?> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of Todo items.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the list of Todo items.
    /// </returns>
    Task<IList<TodoItem>> GetItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of Todo items that match the specified criteria.
    /// </summary>
    /// <param name="prefix">The prefix to search by.</param>
    /// <param name="isCompleted">Whether to search completed items.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the list of matching Todo items.
    /// </returns>
    Task<IList<TodoItem>> FindAsync(string prefix, bool isCompleted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of Todo items created after the specified date and time.
    /// </summary>
    /// <param name="value">The date and time to look for items created after.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the list of matching Todo items.
    /// </returns>
    Task<IList<TodoItem>> GetAfterDateAsync(DateTime value, CancellationToken cancellationToken);
}
