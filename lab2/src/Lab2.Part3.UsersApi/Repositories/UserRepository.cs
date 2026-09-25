using Lab2.Part3.UsersApi.Data;
using Lab2.Part3.UsersApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Part3.UsersApi.Repositories;

/// <summary>
/// Реализация репозитория на EF Core. Изменения сразу сохраняются,
/// так как каждый HTTP-запрос меняет одну запись.
/// </summary>
public class UserRepository : IUserRepository
{
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
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
