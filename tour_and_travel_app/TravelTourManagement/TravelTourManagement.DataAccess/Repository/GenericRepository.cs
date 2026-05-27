using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.DataAccess.Repository;

/// <summary>
/// Generic EF Core repository that provides standard CRUD operations
/// for all entity types in the TravelTourManagement system.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this repository.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object?[] { id }, cancellationToken);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object?[] { id }, cancellationToken) is not null;

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        => predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
}
