namespace Users.Core.Contracts;

/// <summary>
/// Тело запросов на создание, изменение и проверку пользователя.
/// Пример: { "Login": "example_user", "PassHash": "&lt;64 hex-символа SHA-256&gt;" }
/// </summary>
public record UserRequest(string? Login, string? PassHash);
