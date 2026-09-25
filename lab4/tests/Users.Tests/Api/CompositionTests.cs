using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Users.Api.Composition;
using Users.Api.Http;
using Users.Core.Repositories;
using Users.Core.Security;
using Users.Core.Services;
using Users.Data;

namespace Users.Tests.Api;

[TestFixture]
public class CompositionTests
{
    [Test]
    public void AddUsersModule_ResolvesWholeGraphWithGivenOptions()
    {
        var options = new Pbkdf2Options(1_000);
        using var provider = new ServiceCollection().AddUsersModule("DataSource=:memory:", options).BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.That(scope.ServiceProvider.GetRequiredService<IUserService>(), Is.InstanceOf<UserService>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IUserRepository>(), Is.InstanceOf<UserRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IPasswordHashProtector>(), Is.InstanceOf<Pbkdf2PasswordHashProtector>());
        Assert.That(scope.ServiceProvider.GetRequiredService<Pbkdf2Options>(), Is.SameAs(options));
    }

    [Test]
    public void AddUsersModule_WithoutOptions_UsesProductionDefaults()
    {
        using var provider = new ServiceCollection().AddUsersModule("DataSource=:memory:").BuildServiceProvider();

        Assert.That(provider.GetRequiredService<Pbkdf2Options>().Iterations, Is.EqualTo(100_000));
    }

    [Test]
    public void ConfigureJson_KeepsNamesAndCyrillicAndSkipsNulls()
    {
        var options = new JsonSerializerOptions();
        UsersModule.ConfigureJson(options);

        var json = JsonSerializer.Serialize(new ApiResponse<object>(false, "Логин «alice» уже занят", null), options);

        Assert.That(json, Is.EqualTo("{\"Success\":false,\"Message\":\"Логин «alice» уже занят\"}"));
    }
}

[TestFixture]
public class ApiErrorHandlerTests
{
    private static async Task<(int Status, string Body)> Run(Exception? error)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        if (error is not null)
        {
            context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature { Error = error, Path = "/user" });
        }

        await ApiErrorHandler.WriteAsync(context);

        context.Response.Body.Position = 0;
        return (context.Response.StatusCode, await new StreamReader(context.Response.Body).ReadToEndAsync());
    }

    [Test]
    public async Task BrokenJson_Returns400WithOwnMessage()
    {
        var (status, body) = await Run(new BadHttpRequestException("bad json"));

        Assert.That(status, Is.EqualTo(400));
        Assert.That(body, Does.Contain("Некорректный JSON"));
    }

    [Test]
    public async Task AnyOtherError_Returns500WithoutDetails()
    {
        var (status, body) = await Run(new InvalidOperationException("secret stack"));

        Assert.That(status, Is.EqualTo(500));
        Assert.That(body, Does.Not.Contain("secret"));
    }

    [Test]
    public async Task NoErrorFeature_Returns500()
    {
        var (status, _) = await Run(null);

        Assert.That(status, Is.EqualTo(500));
    }
}
