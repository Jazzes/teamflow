using Users.Core.Models;

namespace Users.Core.Repositories;

/// <summary>
/// Доступ к хранилищу пользователей. Реализация живёт в модуле Users.Data.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, занят ли логин. exceptUserId позволяет не учитывать самого пользователя при обновлении.
    /// </summary>
    Task<bool> LoginExistsAsync(string login, int? exceptUserId = null, CancellationToken cancellationToken = default);

    /// <exception cref="DuplicateLoginException">Логин успел занять параллельный запрос.</exception>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <exception cref="DuplicateLoginException">Логин успел занять параллельный запрос.</exception>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
