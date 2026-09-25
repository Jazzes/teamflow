namespace Users.Api.Http;

/// <summary>
/// Единый формат ответа сервера: признак успеха, сообщение, данные и ошибки валидации.
/// </summary>
public record ApiResponse<T>(bool Success, string Message, T? Data, IReadOnlyDictionary<string, string[]>? Errors = null);
