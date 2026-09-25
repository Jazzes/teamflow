using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Users.Api.Http;
using Users.Core.Contracts;
using Users.Core.Services;

namespace Users.Tests.Api;

[TestFixture]
public class HttpResultMapperTests
{
    private static readonly UserResponse Alice = new(1, "alice");

    private static (int? Status, ApiResponse<UserResponse> Body) Map(ServiceResult<UserResponse> result, string? location = null)
    {
        var http = HttpResultMapper.ToHttp(result, location);
        return (((IStatusCodeHttpResult)http).StatusCode, (ApiResponse<UserResponse>)((IValueHttpResult)http).Value!);
    }

    [Test]
    public void ToHttp_Ok_Returns200WithBody()
    {
        var (status, body) = Map(ServiceResult<UserResponse>.Ok("ok", Alice));

        Assert.That(status, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(body, Is.EqualTo(new ApiResponse<UserResponse>(true, "ok", Alice)));
    }

    [Test]
    public void ToHttp_Created_Returns201WithLocation()
    {
        var http = HttpResultMapper.ToHttp(ServiceResult<UserResponse>.Created("created", Alice), "/user/1");

        var created = (Created<ApiResponse<UserResponse>>)http;
        Assert.That(created.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(created.Location, Is.EqualTo("/user/1"));
    }

    [Test]
    public void ToHttp_Invalid_Returns400WithErrors()
    {
        var errors = new Dictionary<string, string[]> { ["Login"] = new[] { "Логин обязателен." } };

        var (status, body) = Map(ServiceResult<UserResponse>.Invalid(errors));

        Assert.That(status, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(body.Success, Is.False);
        Assert.That(body.Errors, Is.SameAs(errors));
    }

    [TestCase(ServiceStatus.NotFound, StatusCodes.Status404NotFound)]
    [TestCase(ServiceStatus.Conflict, StatusCodes.Status409Conflict)]
    [TestCase(ServiceStatus.Unauthorized, StatusCodes.Status401Unauthorized)]
    public void ToHttp_Failures_ReturnMatchingStatus(ServiceStatus serviceStatus, int expected)
    {
        var (status, body) = Map(new ServiceResult<UserResponse>(serviceStatus, "fail", null));

        Assert.That(status, Is.EqualTo(expected));
        Assert.That(body.Message, Is.EqualTo("fail"));
    }

    [Test]
    public void ToHttp_UnknownStatus_Throws()
    {
        var weird = new ServiceResult<UserResponse>((ServiceStatus)999, "?", null);

        Action act = () => HttpResultMapper.ToHttp(weird);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}
