using Users.Core.Contracts;

namespace Users.Core.Services;

/// <summary>
/// Операции над пользователями, не зависящие от способа доставки (HTTP, консоль, тесты).
/// </summary>
public interface IUserService
{
    Task<ServiceResult<UserResponse>> CreateAsync(UserRequest? request, CancellationToken cancellationToken = default);

    Task<ServiceResult<List<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponse>> UpdateAsync(int id, UserRequest? request, CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponse>> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult<UserResponse>> VerifyAsync(UserRequest? request, CancellationToken cancellationToken = default);
}
