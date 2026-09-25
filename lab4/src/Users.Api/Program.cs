using Users.Api.Composition;
using Users.Api.Endpoints;
using Users.Api.Http;
using Users.Core.Security;
using Users.Data;

// Точка сборки приложения: только конфигурация, без бизнес-логики
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("UsersDb")
    ?? throw new InvalidOperationException("Не задана строка подключения UsersDb.");
var iterations = builder.Configuration.GetValue("Pbkdf2:Iterations", 100_000);

builder.Services.AddUsersModule(connectionString, new Pbkdf2Options(iterations));
builder.Services.ConfigureHttpJsonOptions(options => UsersModule.ConfigureJson(options.SerializerOptions));
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();
app.UseExceptionHandler(errorApp => errorApp.Run(ApiErrorHandler.WriteAsync));

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.EnsureCreated();
}

app.MapGet("/", () => Results.Ok(new { Service = "Lab4 Users API", Endpoints = new[] { "POST /user", "GET /user/{id}", "PUT /user/{id}", "DELETE /user/{id}" } }));
app.MapUserEndpoints();

app.Run();
