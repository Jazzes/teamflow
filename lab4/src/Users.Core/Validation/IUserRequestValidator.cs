using Users.Core.Contracts;

namespace Users.Core.Validation;

/// <summary>
/// Проверка входных данных запроса о пользователе.
/// </summary>
public interface IUserRequestValidator
{
    /// <summary>
    /// Возвращает ошибки по полям. Пустой словарь означает, что данные корректны.
    /// </summary>
    IReadOnlyDictionary<string, string[]> Validate(UserRequest? request);
}
