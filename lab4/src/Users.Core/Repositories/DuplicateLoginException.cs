namespace Users.Core.Repositories;

/// <summary>
/// Хранилище отклонило запись из-за повторного логина.
/// Заменяет утечку DbUpdateException из EF Core в бизнес-логику.
/// </summary>
public class DuplicateLoginException : Exception
{
    public string Login { get; }

    public DuplicateLoginException(string login, Exception? inner = null)
        : base($"Логин «{login}» уже занят", inner)
    {
        Login = login;
    }
}
