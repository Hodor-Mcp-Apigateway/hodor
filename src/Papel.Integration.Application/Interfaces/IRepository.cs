namespace Papel.Integration.Application.Interfaces;

using Microsoft.EntityFrameworkCore.Storage;

using Domain.Common;
using System.Linq.Expressions;

/// <summary>
/// Generic repository for entities implementing IPkEntity
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : class, IPkEntity<TKey>
{
    // Query Operations
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);

    // Pagination
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync<TOrderBy>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, TOrderBy>> orderBy, bool ascending, int pageIndex, int pageSize, CancellationToken cancellationToken = default);

    // Aggregation
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    // Modification Operations
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
    void Remove(TKey id);
    void RemoveRange(IEnumerable<TEntity> entities);

    // Transaction and Persistence
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
