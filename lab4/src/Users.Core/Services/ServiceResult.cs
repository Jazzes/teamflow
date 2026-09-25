namespace Users.Core.Services;

/// <summary>
/// Результат операции: статус, сообщение для пользователя, данные и ошибки валидации.
/// </summary>
public record ServiceResult<T>(ServiceStatus Status, string Message, T? Data, IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public bool IsSuccess => Status is ServiceStatus.Ok or ServiceStatus.Created;

    public static ServiceResult<T> Ok(string message, T data) => new(ServiceStatus.Ok, message, data);

    public static ServiceResult<T> Created(string message, T data) => new(ServiceStatus.Created, message, data);

    public static ServiceResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) => new(ServiceStatus.Invalid, "Ошибка валидации", default, errors);

    public static ServiceResult<T> NotFound(string message) => new(ServiceStatus.NotFound, message, default);

    public static ServiceResult<T> Conflict(string message) => new(ServiceStatus.Conflict, message, default);

    public static ServiceResult<T> Unauthorized(string message) => new(ServiceStatus.Unauthorized, message, default);
}
