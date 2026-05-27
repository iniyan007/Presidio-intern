using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Generic base repository interface that defines the standard CRUD contract
/// for all entity repositories in the TravelTourManagement system.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>Gets a single entity by its primary key.</summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities from the data store.</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a filtered, ordered subset of entities.
    /// </summary>
    /// <param name="predicate">Optional WHERE clause predicate.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity and persists it to the data store.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity and persists the changes.</summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes the entity identified by <paramref name="id"/>.</summary>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>Returns <c>true</c> if an entity with the given primary key exists.</summary>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the count of entities that satisfy the optional <paramref name="predicate"/>.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
