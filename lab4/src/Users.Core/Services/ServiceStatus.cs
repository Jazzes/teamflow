namespace Users.Core.Services;

/// <summary>
/// Исход операции сервиса. Модуль Users.Api переводит его в HTTP-код.
/// </summary>
public enum ServiceStatus
{
    Ok,
    Created,
    Invalid,
    NotFound,
    Conflict,
    Unauthorized,
}
