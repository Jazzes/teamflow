using Lab2.DataAccess.Entities;

namespace Lab2.DataAccess.Repositories;

/// <summary>
/// Обобщённый репозиторий: базовый набор CRUD-операций над сущностью.
/// Методы только готовят изменения, а фиксирует их в базе IUnitOfWork.SaveChanges.
/// </summary>
public interface IRepository<T>
    where T : class, IEntity
{
    /// <summary>Возвращает сущность по ключу или null, если её нет.</summary>
    T? GetById(int id);

    IEnumerable<T> GetAll();

    void Add(T entity);

    void Update(T entity);

    /// <summary>Удаляет сущность по ключу. Возвращает false, если такой записи нет.</summary>
    bool Delete(int id);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
