using Lab2.DataAccess.Repositories;

namespace Lab2.DataAccess.UnitOfWork;

/// <summary>
/// Единица работы: даёт доступ к репозиториям, которые делят один контекст,
/// и фиксирует все накопленные изменения одной транзакцией.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IBookRepository Books { get; }

    IAuthorRepository Authors { get; }

    /// <summary>Сохраняет все изменения. Возвращает число затронутых строк.</summary>
    int SaveChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Отбрасывает несохранённые изменения после ошибки.</summary>
    void Rollback();

    /// <summary>Создаёт таблицы, если базы ещё нет.</summary>
    void EnsureDatabaseCreated(bool recreate = false);
}
