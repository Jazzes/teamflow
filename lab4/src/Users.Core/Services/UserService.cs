using Users.Core.Contracts;
using Users.Core.Models;
using Users.Core.Repositories;
using Users.Core.Security;
using Users.Core.Validation;

namespace Users.Core.Services;

/// <summary>
/// Бизнес-логика модуля пользователей. Раньше она была размазана по обработчикам
/// HTTP-маршрутов, теперь собрана здесь и получает все зависимости через конструктор.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUserRequestValidator _validator;
    private readonly IPasswordHashProtector _protector;

    public UserService(IUserRepository users, IUserRequestValidator validator, IPasswordHashProtector protector)
    {
        _users = users;
        _validator = validator;
        _protector = protector;
    }

    public async Task<ServiceResult<UserResponse>> CreateAsync(UserRequest? request, CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(request);
        if (errors.Count > 0)
        {
            return ServiceResult<UserResponse>.Invalid(errors);
        }

        var login = request!.Login!.Trim();
        if (await _users.LoginExistsAsync(login, cancellationToken: cancellationToken))
        {
            return LoginTaken(login);
        }

        var user = new User { Login = login, PassHash = _protector.Protect(request.PassHash!) };
        try
        {
            await _users.AddAsync(user, cancellationToken);
        }
        catch (DuplicateLoginException)
        {
            // Логин успел занять параллельный запрос между проверкой и вставкой
            return LoginTaken(login);
        }

        return ServiceResult<UserResponse>.Created("Пользователь создан", UserResponse.From(user));
    }

    public async Task<ServiceResult<List<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = (await _users.GetAllAsync(cancellationToken)).Select(UserResponse.From).ToList();
        return ServiceResult<List<UserResponse>>.Ok($"Пользователей: {list.Count}", list);
    }

    public async Task<ServiceResult<UserResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        return user is null
            ? NotFound(id)
            : ServiceResult<UserResponse>.Ok("Пользователь найден", UserResponse.From(user));
    }

    public async Task<ServiceResult<UserResponse>> UpdateAsync(int id, UserRequest? request, CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(request);
        if (errors.Count > 0)
        {
            return ServiceResult<UserResponse>.Invalid(errors);
        }

        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(id);
        }

        var login = request!.Login!.Trim();
        if (await _users.LoginExistsAsync(login, exceptUserId: id, cancellationToken: cancellationToken))
        {
            return LoginTaken(login);
        }

        user.Login = login;
        user.PassHash = _protector.Protect(request.PassHash!);
        try
        {
            await _users.UpdateAsync(user, cancellationToken);
        }
        catch (DuplicateLoginException)
        {
            return LoginTaken(login);
        }

        return ServiceResult<UserResponse>.Ok("Данные пользователя обновлены", UserResponse.From(user));
    }

    public async Task<ServiceResult<UserResponse>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(id);
        }

        await _users.DeleteAsync(user, cancellationToken);
        return ServiceResult<UserResponse>.Ok("Пользователь удалён", UserResponse.From(user));
    }

    public async Task<ServiceResult<UserResponse>> VerifyAsync(UserRequest? request, CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(request);
        if (errors.Count > 0)
        {
            return ServiceResult<UserResponse>.Invalid(errors);
        }

        var user = await _users.GetByLoginAsync(request!.Login!.Trim(), cancellationToken);
        if (user is null || !_protector.Verify(request.PassHash!, user.PassHash))
        {
            // Не уточняем, что именно неверно: логин или пароль
            return ServiceResult<UserResponse>.Unauthorized("Неверный логин или пароль");
        }

        return ServiceResult<UserResponse>.Ok("Пароль верный", UserResponse.From(user));
    }

    private static ServiceResult<UserResponse> NotFound(int id)
        => ServiceResult<UserResponse>.NotFound($"Пользователь с Id = {id} не найден");

    private static ServiceResult<UserResponse> LoginTaken(string login)
        => ServiceResult<UserResponse>.Conflict($"Логин «{login}» уже занят");
}
