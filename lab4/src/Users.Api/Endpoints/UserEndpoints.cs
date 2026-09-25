using Users.Api.Http;
using Users.Core.Contracts;
using Users.Core.Services;

namespace Users.Api.Endpoints;

/// <summary>
/// HTTP-маршруты. После рефакторинга обработчики ничего не решают сами:
/// принимают запрос, вызывают IUserService и переводят результат в ответ.
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

    public static async Task<IResult> CreateAsync(UserRequest? request, IUserService users, CancellationToken ct)
    {
        var result = await users.CreateAsync(request, ct);
        return HttpResultMapper.ToHttp(result, result.Data is null ? null : $"/user/{result.Data.Id}");
    }

    public static async Task<IResult> GetAllAsync(IUserService users, CancellationToken ct)
        => HttpResultMapper.ToHttp(await users.GetAllAsync(ct));

    public static async Task<IResult> GetByIdAsync(int id, IUserService users, CancellationToken ct)
        => HttpResultMapper.ToHttp(await users.GetByIdAsync(id, ct));

    public static async Task<IResult> UpdateAsync(int id, UserRequest? request, IUserService users, CancellationToken ct)
        => HttpResultMapper.ToHttp(await users.UpdateAsync(id, request, ct));

    public static async Task<IResult> DeleteAsync(int id, IUserService users, CancellationToken ct)
        => HttpResultMapper.ToHttp(await users.DeleteAsync(id, ct));

    public static async Task<IResult> VerifyAsync(UserRequest? request, IUserService users, CancellationToken ct)
        => HttpResultMapper.ToHttp(await users.VerifyAsync(request, ct));
}
