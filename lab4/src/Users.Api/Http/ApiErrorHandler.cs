using Microsoft.AspNetCore.Diagnostics;

namespace Users.Api.Http;

/// <summary>
/// Обработчик необработанных исключений: отвечает в едином JSON-формате вместо страницы ошибки.
/// </summary>
public static class ApiErrorHandler
{
    public static async Task WriteAsync(HttpContext context)
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var isBadRequest = error is BadHttpRequestException;
        context.Response.StatusCode = isBadRequest ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        var message = isBadRequest ? "Некорректный JSON в теле запроса" : "Внутренняя ошибка сервера";
        await context.Response.WriteAsJsonAsync(new ApiResponse<object>(false, message, null));
    }
}
