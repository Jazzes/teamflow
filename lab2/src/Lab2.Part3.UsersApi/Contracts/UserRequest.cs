namespace Lab2.Part3.UsersApi.Contracts;

/// <summary>
/// Тело запросов POST /user и PUT /user/{id}.
/// Пример: { "Login": "example_user", "PassHash": "&lt;64 hex-символа SHA-256&gt;" }
/// </summary>
public record UserRequest(string? Login, string? PassHash);
