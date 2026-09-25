using Users.Core.Models;

namespace Users.Core.Contracts;

/// <summary>
/// Данные пользователя, которые отдаются клиенту. Хеш пароля наружу не выходит.
/// </summary>
public record UserResponse(int Id, string Login)
{
    public static UserResponse From(User user) => new(user.Id, user.Login);
}
