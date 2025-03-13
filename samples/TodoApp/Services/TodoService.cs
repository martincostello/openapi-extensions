// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

/// <summary>
/// A class representing a service for managing Todo items.
/// </summary>
/// <param name="repository">The <see cref="ITodoRepository"/> to use.</param>
public class TodoService(ITodoRepository repository) : ITodoService
{
    /// <inheritdoc/>
    public async Task<string> AddItemAsync(string text, CancellationToken cancellationToken)
    {
        var item = await repository.AddItemAsync(text, cancellationToken);
        return item.Id.ToString();
    }

    /// <inheritdoc/>
    public async Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await repository.CompleteItemAsync(itemId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await repository.DeleteItemAsync(itemId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TodoItemModel?> GetAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await repository.GetItemAsync(itemId, cancellationToken);
        return item is null ? null : MapItem(item);
    }

    /// <inheritdoc/>
    public async Task<TodoListViewModel> GetListAsync(CancellationToken cancellationToken)
    {
        var items = await repository.GetItemsAsync(cancellationToken);
        return MapItems(items);
    }

    /// <inheritdoc/>
    public async Task<TodoListViewModel> FindAsync(TodoItemFilterModel filter, CancellationToken cancellationToken)
    {
        var items = await repository.FindAsync(filter.Text, filter.IsCompleted, cancellationToken);
        return MapItems(items);
    }

    /// <inheritdoc/>
    public async Task<TodoListViewModel> GetAfterDateAsync(DateTime value, CancellationToken cancellationToken)
    {
        var items = await repository.GetAfterDateAsync(value, cancellationToken);
        return MapItems(items);
    }

    private static TodoListViewModel MapItems(IList<TodoItem> items)
    {
        var result = new List<TodoItemModel>(items.Count);

        foreach (var todo in items)
        {
            result.Add(MapItem(todo));
        }

        return new() { Items = result };
    }

    private static TodoItemModel MapItem(TodoItem item)
    {
        return new()
        {
            Id = item.Id.ToString(),
            IsCompleted = item.CompletedAt.HasValue,
            LastUpdated = (item.CompletedAt ?? item.CreatedAt).ToString("u", CultureInfo.InvariantCulture),
            Text = item.Text,
        };
    }
}
