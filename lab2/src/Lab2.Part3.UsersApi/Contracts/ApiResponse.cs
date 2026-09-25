namespace Lab2.Part3.UsersApi.Contracts;

/// <summary>
/// Единый формат ответа сервера: признак успеха, сообщение, данные и ошибки валидации.
/// </summary>
public record ApiResponse<T>(bool Success, string Message, T? Data, IDictionary<string, string[]>? Errors = null)
{
    public static ApiResponse<T> Ok(string message, T data) => new(true, message, data);

    public static ApiResponse<T> Fail(string message, IDictionary<string, string[]>? errors = null)
        => new(false, message, default, errors);
}
