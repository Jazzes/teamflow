using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Lab2.Part3.UsersApi.Contracts;
using Lab2.Part3.UsersApi.Data;
using Lab2.Part3.UsersApi.Endpoints;
using Lab2.Part3.UsersApi.Repositories;
using Lab2.Part3.UsersApi.Security;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Строка подключения берётся из appsettings.json или переменной окружения ConnectionStrings__UsersDb
var connectionString = builder.Configuration.GetConnectionString("UsersDb")
    ?? throw new InvalidOperationException("Не задана строка подключения UsersDb.");

builder.Services.AddDbContext<UsersDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHashProtector, Pbkdf2PasswordHashProtector>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Имена полей как в задании (Login, PassHash), кириллица и кавычки-ёлочки в ответах не экранируются
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.Cyrillic);
    // Пустые поля (например, Errors при успешном ответе) в JSON не выводятся
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Ошибки разбора JSON превращаются в исключение, которое ниже оборачивается в наш формат ответа
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var isBadRequest = error is BadHttpRequestException;
    context.Response.StatusCode = isBadRequest ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
    var message = isBadRequest ? "Некорректный JSON в теле запроса" : "Внутренняя ошибка сервера";
    await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message));
}));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Ok(new { Service = "Lab2 Users API", Endpoints = new[] { "POST /user", "GET /user/{id}", "PUT /user/{id}", "DELETE /user/{id}" } }));
app.MapUserEndpoints();

app.Run();
