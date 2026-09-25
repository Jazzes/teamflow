using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Users.Api.Composition;
using Users.Api.Endpoints;
using Users.Api.Http;
using Users.Core.Contracts;
using Users.Core.Services;

namespace Users.Tests.Api;

/// <summary>
/// Обработчики маршрутов вызываются напрямую с подменённым IUserService:
/// проверяется только связка «запрос → сервис → HTTP-ответ».
/// </summary>
[TestFixture]
public class UserEndpointsTests
{
    private static readonly UserResponse Alice = new(5, "alice");
    private IUserService _service = null!;

    [SetUp]
    public void SetUp() => _service = Substitute.For<IUserService>();

    private static int? StatusOf(IResult result) => ((IStatusCodeHttpResult)result).StatusCode;

    [Test]
    public async Task Create_Success_PointsLocationToNewUser()
    {
        _service.CreateAsync(Arg.Any<UserRequest?>(), Arg.Any<CancellationToken>()).Returns(ServiceResult<UserResponse>.Created("c", Alice));

        var result = await UserEndpoints.CreateAsync(TestData.Request(), _service, CancellationToken.None);

        Assert.That(((Created<ApiResponse<UserResponse>>)result).Location, Is.EqualTo("/user/5"));
    }

    [Test]
    public async Task Create_Failure_HasNoLocation()
    {
        _service.CreateAsync(Arg.Any<UserRequest?>(), Arg.Any<CancellationToken>()).Returns(ServiceResult<UserResponse>.Conflict("taken"));

        var result = await UserEndpoints.CreateAsync(TestData.Request(), _service, CancellationToken.None);

        Assert.That(StatusOf(result), Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public async Task OtherHandlers_DelegateToServiceAndMapResult()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns(ServiceResult<List<UserResponse>>.Ok("all", new List<UserResponse> { Alice }));
        _service.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(ServiceResult<UserResponse>.Ok("one", Alice));
        _service.UpdateAsync(5, Arg.Any<UserRequest?>(), Arg.Any<CancellationToken>()).Returns(ServiceResult<UserResponse>.NotFound("none"));
        _service.DeleteAsync(5, Arg.Any<CancellationToken>()).Returns(ServiceResult<UserResponse>.Ok("deleted", Alice));
        _service.VerifyAsync(Arg.Any<UserRequest?>(), Arg.Any<CancellationToken>()).Returns(ServiceResult<UserResponse>.Unauthorized("bad"));

        Assert.That(StatusOf(await UserEndpoints.GetAllAsync(_service, CancellationToken.None)), Is.EqualTo(200));
        Assert.That(StatusOf(await UserEndpoints.GetByIdAsync(5, _service, CancellationToken.None)), Is.EqualTo(200));
        Assert.That(StatusOf(await UserEndpoints.UpdateAsync(5, TestData.Request(), _service, CancellationToken.None)), Is.EqualTo(404));
        Assert.That(StatusOf(await UserEndpoints.DeleteAsync(5, _service, CancellationToken.None)), Is.EqualTo(200));
        Assert.That(StatusOf(await UserEndpoints.VerifyAsync(TestData.Request(), _service, CancellationToken.None)), Is.EqualTo(401));
    }

    [Test]
    public void MapUserEndpoints_RegistersSixRoutes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddUsersModule("DataSource=:memory:");
        var app = builder.Build();

        app.MapUserEndpoints();

        var routes = ((IEndpointRouteBuilder)app).DataSources.SelectMany(s => s.Endpoints).OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText).ToList();
        Assert.That(routes, Has.Count.EqualTo(6));
        Assert.That(routes, Does.Contain("/user/{id:int}"));
        Assert.That(routes, Does.Contain("/user/verify"));
    }
}
