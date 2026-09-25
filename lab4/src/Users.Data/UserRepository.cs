using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Users.Core.Models;
using Users.Core.Repositories;

namespace Users.Data;

/// <summary>
/// Реализация репозитория на EF Core. Изменения сразу сохраняются,
/// так как каждый HTTP-запрос меняет одну запись.
/// </summary>
public class UserRepository : IUserRepository
{
    /// <summary>Расширенный код SQLite для нарушения уникального ограничения.</summary>
    private const int SqliteConstraintUnique = 2067;

    private readonly UsersDbContext _db;

    public UserRepository(UsersDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(cancellationToken);

    public Task<bool> LoginExistsAsync(string login, int? exceptUserId = null, CancellationToken cancellationToken = default)
        => _db.Users.AnyAsync(u => u.Login == login && (exceptUserId == null || u.Id != exceptUserId), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
        await SaveAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Update(user);
        await SaveAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Сохраняет изменения и переводит ошибку уникальности SQLite в исключение предметной области,
    /// чтобы бизнес-логика не зависела от EF Core.
    /// </summary>
    private async Task SaveAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique })
        {
            // Неудачную запись убираем из трекера, иначе она уйдёт в базу при следующем сохранении
            _db.Entry(user).State = EntityState.Detached;
            throw new DuplicateLoginException(user.Login, ex);
        }
    }
}
