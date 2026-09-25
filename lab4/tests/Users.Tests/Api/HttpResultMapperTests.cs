using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Users.Api.Http;
using Users.Core.Contracts;
using Users.Core.Services;

namespace Users.Tests.Api;

[TestFixture]
public class HttpResultMapperTests
{
    [Test]
    public void ToHttp_Ok_Returns200WithBody()
    {
        var result = HttpResultMapper.ToHttp(ServiceResult<UserResponse>.Ok("ok", new UserResponse(1, "alice")));

        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        var body = (ApiResponse<UserResponse>)((IValueHttpResult)result).Value!;
        Assert.That(body.Success, Is.True);
    }
}
