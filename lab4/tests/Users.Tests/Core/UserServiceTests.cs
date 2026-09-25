using Users.Core.Contracts;
using Users.Core.Models;
using Users.Core.Repositories;
using Users.Core.Security;
using Users.Core.Services;
using Users.Core.Validation;

namespace Users.Tests.Core;

/// <summary>
/// Бизнес-логика проверяется в изоляции: репозиторий, валидатор и защитник хеша подменены через NSubstitute.
/// </summary>
[TestFixture]
public class UserServiceTests
{
    private static readonly IReadOnlyDictionary<string, string[]> SomeErrors =
        new Dictionary<string, string[]> { ["Login"] = new[] { "Логин обязателен." } };

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

    private void MakeInvalid() => _validator.Validate(Arg.Any<UserRequest?>()).Returns(SomeErrors);

    // ---------- создание

    [Test]
    public async Task Create_ValidRequest_SavesUserAndReturnsCreated()
    {
        var result = await _service.CreateAsync(TestData.Request(" alice "));

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Created));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data!.Login, Is.EqualTo("alice"));
        await _repo.Received(1).AddAsync(Arg.Is<User>(u => u.Login == "alice" && u.PassHash == "protected"), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_InvalidRequest_ReturnsInvalidAndDoesNotTouchRepository()
    {
        MakeInvalid();

        var result = await _service.CreateAsync(null);

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Invalid));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Is.SameAs(SomeErrors));
        Assert.That(_repo.ReceivedCalls(), Is.Empty);
    }

    [Test]
    public async Task Create_LoginAlreadyExists_ReturnsConflict()
    {
        _repo.LoginExistsAsync("alice", null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.CreateAsync(TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Conflict));
        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Create_ParallelInsertWithSameLogin_ReturnsConflict()
    {
        _repo.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new DuplicateLoginException("alice")));

        var result = await _service.CreateAsync(TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Conflict));
        Assert.That(result.Message, Does.Contain("alice"));
    }

    // ---------- чтение

    [Test]
    public async Task GetAll_ReturnsMappedUsersAndCount()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User>
        {
            new() { Id = 1, Login = "alice", PassHash = "x" },
            new() { Id = 2, Login = "bob", PassHash = "y" },
        });

        var result = await _service.GetAllAsync();

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Ok));
        Assert.That(result.Message, Is.EqualTo("Пользователей: 2"));
        Assert.That(result.Data, Is.EqualTo(new[] { new UserResponse(1, "alice"), new UserResponse(2, "bob") }));
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
    public async Task GetById_MissingUser_ReturnsNotFound()
    {
        var result = await _service.GetByIdAsync(42);

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.NotFound));
        Assert.That(result.Message, Does.Contain("42"));
    }

    // ---------- изменение

    [Test]
    public async Task Update_ValidRequest_ChangesLoginAndHash()
    {
        var user = new User { Id = 3, Login = "old", PassHash = "old-hash" };
        _repo.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _service.UpdateAsync(3, TestData.Request("new_login", TestData.OtherHash));

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Ok));
        Assert.That(user.Login, Is.EqualTo("new_login"));
        Assert.That(user.PassHash, Is.EqualTo("protected"));
        _protector.Received(1).Protect(TestData.OtherHash);
        await _repo.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_InvalidRequest_ReturnsInvalid()
    {
        MakeInvalid();

        var result = await _service.UpdateAsync(3, TestData.Request(""));

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Invalid));
        await _repo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_MissingUser_ReturnsNotFound()
    {
        var result = await _service.UpdateAsync(3, TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.NotFound));
    }

    [Test]
    public async Task Update_LoginTakenByAnotherUser_ReturnsConflict()
    {
        _repo.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns(new User { Id = 3, Login = "old" });
        _repo.LoginExistsAsync("alice", 3, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.UpdateAsync(3, TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Conflict));
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_ParallelUpdateWithSameLogin_ReturnsConflict()
    {
        _repo.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns(new User { Id = 3, Login = "old" });
        _repo.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new DuplicateLoginException("alice")));

        var result = await _service.UpdateAsync(3, TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Conflict));
    }

    // ---------- удаление

    [Test]
    public async Task Delete_ExistingUser_RemovesAndReturnsOk()
    {
        var user = new User { Id = 5, Login = "bob" };
        _repo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _service.DeleteAsync(5);

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Ok));
        await _repo.Received(1).DeleteAsync(user, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Delete_MissingUser_ReturnsNotFound()
    {
        var result = await _service.DeleteAsync(5);

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.NotFound));
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    // ---------- проверка пароля

    [Test]
    public async Task Verify_CorrectPassword_ReturnsOk()
    {
        _repo.GetByLoginAsync("alice", Arg.Any<CancellationToken>()).Returns(new User { Id = 1, Login = "alice", PassHash = "stored" });
        _protector.Verify(TestData.Hash, "stored").Returns(true);

        var result = await _service.VerifyAsync(TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Ok));
    }

    [Test]
    public async Task Verify_InvalidRequest_ReturnsInvalid()
    {
        MakeInvalid();

        var result = await _service.VerifyAsync(TestData.Request("x"));

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Invalid));
    }

    [Test]
    public async Task Verify_UnknownLogin_ReturnsUnauthorized()
    {
        var result = await _service.VerifyAsync(TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Unauthorized));
        _protector.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task Verify_WrongPassword_ReturnsUnauthorizedWithSameMessage()
    {
        _repo.GetByLoginAsync("alice", Arg.Any<CancellationToken>()).Returns(new User { Id = 1, Login = "alice", PassHash = "stored" });
        _protector.Verify(TestData.Hash, "stored").Returns(false);

        var result = await _service.VerifyAsync(TestData.Request());

        Assert.That(result.Status, Is.EqualTo(ServiceStatus.Unauthorized));
        Assert.That(result.Message, Is.EqualTo("Неверный логин или пароль"));
    }
}
