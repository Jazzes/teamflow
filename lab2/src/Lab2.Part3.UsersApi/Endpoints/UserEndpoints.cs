using Lab2.Part3.UsersApi.Contracts;
using Lab2.Part3.UsersApi.Models;
using Lab2.Part3.UsersApi.Repositories;
using Lab2.Part3.UsersApi.Security;
using Lab2.Part3.UsersApi.Validation;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Part3.UsersApi.Endpoints;

/// <summary>
/// HTTP-маршруты для CRUD над пользователями. Формат запросов и ответов JSON.
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user");

        group.MapPost("/", CreateAsync);
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapPost("/verify", VerifyAsync);

        return app;
    }

    /// <summary>POST /user: создание пользователя.</summary>
    private static async Task<IResult> CreateAsync(
        UserRequest? request, IUserRepository users, IPasswordHashProtector protector, CancellationToken ct)
    {
        var errors = UserRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(ApiResponse<UserResponse>.Fail("Ошибка валидации", errors));
        }

        var login = request!.Login!.Trim();
        if (await users.LoginExistsAsync(login, cancellationToken: ct))
        {
            return LoginTaken(login);
        }

        var user = new User { Login = login, PassHash = protector.Protect(request.PassHash!) };
        try
        {
            await users.AddAsync(user, ct);
        }
        catch (DbUpdateException)
        {
            // Сюда попадём, если такой же логин успел записать параллельный запрос
            return LoginTaken(login);
        }

        return Results.Created($"/user/{user.Id}", ApiResponse<UserResponse>.Ok("Пользователь создан", UserResponse.From(user)));
    }

    /// <summary>GET /user: список пользователей (дополнительный маршрут для удобства проверки).</summary>
    private static async Task<IResult> GetAllAsync(IUserRepository users, CancellationToken ct)
    {
        var list = (await users.GetAllAsync(ct)).Select(UserResponse.From).ToList();
        return Results.Ok(ApiResponse<List<UserResponse>>.Ok($"Пользователей: {list.Count}", list));
    }

    /// <summary>GET /user/{id}: пользователь по идентификатору.</summary>
    private static async Task<IResult> GetByIdAsync(int id, IUserRepository users, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null
            ? NotFound(id)
            : Results.Ok(ApiResponse<UserResponse>.Ok("Пользователь найден", UserResponse.From(user)));
    }

    /// <summary>PUT /user/{id}: замена логина и хеша пароля.</summary>
    private static async Task<IResult> UpdateAsync(
        int id, UserRequest? request, IUserRepository users, IPasswordHashProtector protector, CancellationToken ct)
    {
        var errors = UserRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(ApiResponse<UserResponse>.Fail("Ошибка валидации", errors));
        }

        var user = await users.GetByIdAsync(id, ct);
        if (user is null)
        {
            return NotFound(id);
        }

        var login = request!.Login!.Trim();
        if (await users.LoginExistsAsync(login, exceptUserId: id, cancellationToken: ct))
        {
            return LoginTaken(login);
        }

        user.Login = login;
        user.PassHash = protector.Protect(request.PassHash!);
        try
        {
            await users.UpdateAsync(user, ct);
        }
        catch (DbUpdateException)
        {
            return LoginTaken(login);
        }

        return Results.Ok(ApiResponse<UserResponse>.Ok("Данные пользователя обновлены", UserResponse.From(user)));
    }

    /// <summary>DELETE /user/{id}: удаление пользователя.</summary>
    private static async Task<IResult> DeleteAsync(int id, IUserRepository users, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null)
        {
            return NotFound(id);
        }

        await users.DeleteAsync(user, ct);
        return Results.Ok(ApiResponse<UserResponse>.Ok("Пользователь удалён", UserResponse.From(user)));
    }

    /// <summary>POST /user/verify: проверка пары логин + хеш пароля.</summary>
    private static async Task<IResult> VerifyAsync(
        UserRequest? request, IUserRepository users, IPasswordHashProtector protector, CancellationToken ct)
    {
        var errors = UserRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(ApiResponse<UserResponse>.Fail("Ошибка валидации", errors));
        }

        var user = await users.GetByLoginAsync(request!.Login!.Trim(), ct);
        if (user is null || !protector.Verify(request.PassHash!, user.PassHash))
        {
            // Не уточняем, что именно неверно: логин или пароль
            return Results.Json(ApiResponse<UserResponse>.Fail("Неверный логин или пароль"), statusCode: StatusCodes.Status401Unauthorized);
        }

        return Results.Ok(ApiResponse<UserResponse>.Ok("Пароль верный", UserResponse.From(user)));
    }

    private static IResult NotFound(int id)
        => Results.NotFound(ApiResponse<UserResponse>.Fail($"Пользователь с Id = {id} не найден"));

    private static IResult LoginTaken(string login)
        => Results.Conflict(ApiResponse<UserResponse>.Fail($"Логин «{login}» уже занят"));
}
