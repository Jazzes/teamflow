using System.Text.RegularExpressions;
using Lab2.Part3.UsersApi.Contracts;

namespace Lab2.Part3.UsersApi.Validation;

/// <summary>
/// Проверка входных данных до обращения к базе.
/// </summary>
public static partial class UserRequestValidator
{
    public const int LoginMinLength = 3;
    public const int LoginMaxLength = 32;

    /// <summary>
    /// Возвращает словарь ошибок по полям. Пустой словарь означает, что данные корректны.
    /// </summary>
    public static Dictionary<string, string[]> Validate(UserRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors["Body"] = new[] { "Тело запроса пустое или не является JSON." };
            return errors;
        }

        var login = request.Login?.Trim();
        if (string.IsNullOrEmpty(login))
        {
            errors["Login"] = new[] { "Логин обязателен." };
        }
        else if (login.Length < LoginMinLength || login.Length > LoginMaxLength)
        {
            errors["Login"] = new[] { $"Длина логина от {LoginMinLength} до {LoginMaxLength} символов." };
        }
        else if (!LoginRegex().IsMatch(login))
        {
            errors["Login"] = new[] { "Логин может содержать только латинские буквы, цифры, точку, дефис и подчёркивание." };
        }

        if (string.IsNullOrEmpty(request.PassHash))
        {
            errors["PassHash"] = new[] { "Хеш пароля обязателен." };
        }
        else if (!Sha256HexRegex().IsMatch(request.PassHash))
        {
            errors["PassHash"] = new[] { "Ожидается хеш SHA-256 в виде 64 шестнадцатеричных символов. Открытый пароль не принимается." };
        }

        return errors;
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]+$")]
    private static partial Regex LoginRegex();

    [GeneratedRegex("^[0-9a-fA-F]{64}$")]
    private static partial Regex Sha256HexRegex();
}
