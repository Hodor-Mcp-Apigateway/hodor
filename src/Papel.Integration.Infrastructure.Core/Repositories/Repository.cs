namespace Papel.Integration.Infrastructure.Core.Repositories;

using System.Linq.Expressions;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Papel.Integration.Application.Common.Interfaces;
using Papel.Integration.Domain.Common;


public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IPkEntity<TKey>
{
    protected readonly IApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(IApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // StatusId kontrolü varsa ekle
        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.FirstOrDefaultAsync(param => EqualityComparer<TKey>.Default.Equals(param.Id, id), cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.FirstOrDefaultAsync(param => EqualityComparer<TKey>.Default.Equals(param.Id, id));
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(predicate);

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.Where(predicate);
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(predicate);

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync<TOrderBy>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TOrderBy>> orderBy,
        bool ascending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(predicate);

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(param => (param).StatusId == Status.Valid);
        }

        return await query.AnyAsync(param => EqualityComparer<TKey>.Default.Equals(param.Id, id), cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await _dbSet.AddRangeAsync(entityList, cancellationToken);
        return entityList;
    }

    public virtual void Update(TEntity entity)
    {
        // IEntity implement eden entity'ler için ModifDate güncellemesi
        if (entity is IEntity baseEntity)
        {
            baseEntity.ModifDate = DateTime.Now;
            baseEntity.ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
        }

        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity is IEntity baseEntity)
            {
                baseEntity.ModifDate = DateTime.Now;
                baseEntity.ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
            }
        }
        _dbSet.UpdateRange(entities);
    }

    public virtual void Remove(TEntity entity)
    {
        // Soft delete for IEntity implementing entities
        if (entity is IEntity baseEntity)
        {
            baseEntity.StatusId = Status.Invalid;
            baseEntity.ModifDate = DateTime.Now;
            baseEntity.ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual void Remove(TKey id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            Remove(entity);
        }
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity is IEntity baseEntity)
            {
                baseEntity.StatusId = Status.Invalid;
                baseEntity.ModifDate = DateTime.Now;
                baseEntity.ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
            }
        }

        if (entities.Any() && entities.First() is IEntity)
        {
            _dbSet.UpdateRange(entities);
        }
        else
        {
            _dbSet.RemoveRange(entities);
        }
    }

    public virtual async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
