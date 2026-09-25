using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using Users.Core.Repositories;
using Users.Core.Security;
using Users.Core.Services;
using Users.Core.Validation;
using Users.Data;

namespace Users.Api.Composition;

/// <summary>
/// Регистрация зависимостей модуля. Конкретные классы связываются с интерфейсами только здесь.
/// </summary>
public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, string connectionString, Pbkdf2Options? pbkdf2 = null)
    {
        services.AddDbContext<UsersDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IUserRequestValidator, UserRequestValidator>();
        services.AddSingleton<ISaltGenerator, RandomSaltGenerator>();
        services.AddSingleton(pbkdf2 ?? new Pbkdf2Options());
        services.AddSingleton<IPasswordHashProtector, Pbkdf2PasswordHashProtector>();
        return services;
    }

    /// <summary>
    /// Настройки JSON: имена полей как в задании (Login, PassHash), кириллица без экранирования, пустые поля не выводятся.
    /// </summary>
    public static void ConfigureJson(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = null;
        options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.Cyrillic);
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}
