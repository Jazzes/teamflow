using Lab2.Part3.UsersApi.Models;

namespace Lab2.Part3.UsersApi.Repositories;

/// <summary>
/// Доступ к таблице пользователей. Все методы асинхронные.
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

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
