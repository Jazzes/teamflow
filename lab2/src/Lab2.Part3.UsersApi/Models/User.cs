namespace Lab2.Part3.UsersApi.Models;

/// <summary>
/// Пользователь. Соответствует таблице Users с полями Id, Login, PassHash.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля. Клиент присылает SHA-256 от пароля, сервер дополнительно
    /// пропускает его через PBKDF2 с солью и хранит результат в этом поле.
    /// </summary>
    public string PassHash { get; set; } = string.Empty;
}
