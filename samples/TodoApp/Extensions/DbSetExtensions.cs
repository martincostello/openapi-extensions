// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// A class containing extension ethods for <see cref="DbSet{T}"/>.
/// </summary>
public static class DbSetExtensions
{
    /// <summary>
    /// Finds the item with the specified ID.
    /// </summary>
    /// <typeparam name="TEntity">The type of the item.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="set">The set to search for the item in.</param>
    /// <param name="keyValue">The key's value.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation to find the item.
    /// </returns>
    public static ValueTask<TEntity?> FindItemAsync<TEntity, TKey>(
        this DbSet<TEntity> set,
        TKey keyValue,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(keyValue);
        return set.FindAsync([keyValue], cancellationToken);
    }
}
