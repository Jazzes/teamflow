using System.Text.RegularExpressions;
using Users.Core.Contracts;

namespace Users.Core.Validation;

/// <summary>
/// Проверка логина и формата хеша пароля до обращения к базе.
/// </summary>
public partial class UserRequestValidator : IUserRequestValidator
{
    public const int LoginMinLength = 3;
    public const int LoginMaxLength = 32;

    public IReadOnlyDictionary<string, string[]> Validate(UserRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors["Body"] = new[] { "Тело запроса пустое или не является JSON." };
            return errors;
        }

        var loginError = ValidateLogin(request.Login?.Trim());
        if (loginError is not null)
        {
            errors["Login"] = new[] { loginError };
        }

        var hashError = ValidatePassHash(request.PassHash);
        if (hashError is not null)
        {
            errors["PassHash"] = new[] { hashError };
        }

        return errors;
    }

    private static string? ValidateLogin(string? login)
    {
        if (string.IsNullOrEmpty(login))
        {
            return "Логин обязателен.";
        }

        if (login.Length < LoginMinLength || login.Length > LoginMaxLength)
        {
            return $"Длина логина от {LoginMinLength} до {LoginMaxLength} символов.";
        }

        return LoginRegex().IsMatch(login)
            ? null
            : "Логин может содержать только латинские буквы, цифры, точку, дефис и подчёркивание.";
    }

    private static string? ValidatePassHash(string? passHash)
    {
        if (string.IsNullOrEmpty(passHash))
        {
            return "Хеш пароля обязателен.";
        }

        return Sha256HexRegex().IsMatch(passHash)
            ? null
            : "Ожидается хеш SHA-256 в виде 64 шестнадцатеричных символов. Открытый пароль не принимается.";
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]+$")]
    private static partial Regex LoginRegex();

    [GeneratedRegex("^[0-9a-fA-F]{64}$")]
    private static partial Regex Sha256HexRegex();
}
