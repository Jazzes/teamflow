using Users.Core.Services;

namespace Users.Api.Http;

/// <summary>
/// Переводит результат сервиса в HTTP-ответ. Вся логика выбора кода состояния собрана в одном месте.
/// </summary>
public static class HttpResultMapper
{
    public static IResult ToHttp<T>(ServiceResult<T> result, string? location = null)
    {
        var body = new ApiResponse<T>(result.IsSuccess, result.Message, result.Data, result.Errors);
        return result.Status switch
        {
            ServiceStatus.Ok => Results.Ok(body),
            ServiceStatus.Created => Results.Created(location, body),
            ServiceStatus.Invalid => Results.BadRequest(body),
            ServiceStatus.NotFound => Results.NotFound(body),
            ServiceStatus.Conflict => Results.Conflict(body),
            ServiceStatus.Unauthorized => Results.Json(body, statusCode: StatusCodes.Status401Unauthorized),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Неизвестный статус результата"),
        };
    }
}
