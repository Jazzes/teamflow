using Lab2.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab2.DataAccess.Repositories;

/// <summary>
/// Универсальная реализация IRepository для любой сущности на EF Core.
/// Специализированные репозитории наследуются от неё и добавляют свои запросы.
/// </summary>
public class GenericRepository<T> : IRepository<T>
    where T : class, IEntity
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(DbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual T? GetById(int id) => Set.Find(id);

    public virtual IEnumerable<T> GetAll() => Set.AsNoTracking().OrderBy(e => e.Id).ToList();

    public virtual void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Set.Add(entity);
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Set.Update(entity);
    }

    public virtual bool Delete(int id)
    {
        var entity = Set.Find(id);
        if (entity is null)
        {
            return false;
        }

        Set.Remove(entity);
        return true;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Set.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Set.AsNoTracking().OrderBy(e => e.Id).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Set.AddAsync(entity, cancellationToken);
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await Set.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        Set.Remove(entity);
        return true;
    }
}
