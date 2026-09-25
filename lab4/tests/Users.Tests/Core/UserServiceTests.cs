using Users.Core.Contracts;
using Users.Core.Models;
using Users.Core.Repositories;
using Users.Core.Security;
using Users.Core.Services;
using Users.Core.Validation;

namespace Users.Tests.Core;

[TestFixture]
public class UserServiceTests
{
    private IUserRepository _repo = null!;
    private IUserRequestValidator _validator = null!;
    private IPasswordHashProtector _protector = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = Substitute.For<IUserRepository>();
        _validator = Substitute.For<IUserRequestValidator>();
        _protector = Substitute.For<IPasswordHashProtector>();
        _validator.Validate(Arg.Any<UserRequest?>()).Returns(TestData.NoErrors);
        _protector.Protect(Arg.Any<string>()).Returns("protected");
        _service = new UserService(_repo, _validator, _protector);
    }

    [Test]
    public async Task Create_ValidRequest_SavesUserAndReturnsCreated()
    {
        var result = await _service.CreateAsync(TestData.Request(" alice "));

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Created));
        Assert.That(result.Data!.Login, Is.EqualTo("alice"));
        await _repo.Received(1).AddAsync(Arg.Is<User>(u => u.Login == "alice" && u.PassHash == "protected"), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetById_ExistingUser_ReturnsOk()
    {
        _repo.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(new User { Id = 7, Login = "bob" });

        var result = await _service.GetByIdAsync(7);

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Ok));
        Assert.That(result.Data, Is.EqualTo(new UserResponse(7, "bob")));
    }

    [Test]
    public async Task Verify_CorrectPassword_ReturnsOk()
    {
        _repo.GetByLoginAsync("alice", Arg.Any<CancellationToken>()).Returns(new User { Id = 1, Login = "alice", PassHash = "stored" });
        _protector.Verify(TestData.Hash, "stored").Returns(true);

        var result = await _service.VerifyAsync(TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Ok));
    }
}
