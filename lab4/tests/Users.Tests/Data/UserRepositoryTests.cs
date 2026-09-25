using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Users.Core.Models;
using Users.Core.Repositories;
using Users.Data;

namespace Users.Tests.Data;

/// <summary>
/// Тесты репозитория на настоящей SQLite в памяти: проверяются запросы и ограничения схемы.
/// Каждый тест получает свою чистую базу.
/// </summary>
[TestFixture]
public class UserRepositoryTests
{
    private SqliteConnection _connection = null!;
    private UsersDbContext _db = null!;
    private UserRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _repo = new UserRepository(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<User> AddUser(string login)
    {
        var user = new User { Login = login, PassHash = "hash-" + login };
        await _repo.AddAsync(user);
        return user;
    }

    [Test]
    public async Task Add_ThenGetById_ReturnsSavedUser()
    {
        var user = await AddUser("alice");

        var loaded = await _repo.GetByIdAsync(user.Id);

        Assert.That(loaded!.Login, Is.EqualTo("alice"));
    }

    [Test]
    public async Task GetById_Missing_ReturnsNull()
    {
        Assert.That(await _repo.GetByIdAsync(404), Is.Null);
    }

    [Test]
    public async Task GetByLogin_IgnoresCase()
    {
        await AddUser("Alice");

        var loaded = await _repo.GetByLoginAsync("ALICE");

        Assert.That(loaded!.Login, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task GetAll_ReturnsUsersOrderedById()
    {
        await AddUser("bob");
        await AddUser("alice");

        var all = await _repo.GetAllAsync();

        Assert.That(all.Select(u => u.Login), Is.EqualTo(new[] { "bob", "alice" }));
    }

    [Test]
    public async Task LoginExists_WithoutException_FindsAnyUser()
    {
        await AddUser("alice");

        Assert.That(await _repo.LoginExistsAsync("ALICE"), Is.True);
        Assert.That(await _repo.LoginExistsAsync("carol"), Is.False);
    }

    [Test]
    public async Task LoginExists_ExceptSelf_IgnoresTheSameUser()
    {
        var alice = await AddUser("alice");
        var bob = await AddUser("bob");

        Assert.That(await _repo.LoginExistsAsync("alice", alice.Id), Is.False);
        Assert.That(await _repo.LoginExistsAsync("alice", bob.Id), Is.True);
    }

    [Test]
    public async Task Update_ChangesStoredValues()
    {
        var user = await AddUser("alice");
        user.PassHash = "new-hash";

        await _repo.UpdateAsync(user);

        _db.ChangeTracker.Clear();
        Assert.That((await _repo.GetByIdAsync(user.Id))!.PassHash, Is.EqualTo("new-hash"));
    }

    [Test]
    public async Task Delete_RemovesUser()
    {
        var user = await AddUser("alice");

        await _repo.DeleteAsync(user);

        Assert.That(await _repo.GetAllAsync(), Is.Empty);
    }

    [Test]
    public async Task Add_DuplicateLoginInAnyCase_ThrowsDomainExceptionAndDetachesEntity()
    {
        await AddUser("alice");
        var duplicate = new User { Login = "ALICE", PassHash = "h" };

        Func<Task> act = () => _repo.AddAsync(duplicate);

        var ex = Assert.ThrowsAsync<DuplicateLoginException>(act);

        Assert.That(ex!.Login, Is.EqualTo("ALICE"));
        Assert.That(ex.InnerException, Is.InstanceOf<DbUpdateException>());
        Assert.That(_db.Entry(duplicate).State, Is.EqualTo(EntityState.Detached));
        Assert.That(await _repo.GetAllAsync(), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Update_ToExistingLogin_ThrowsDomainException()
    {
        await AddUser("alice");
        var bob = await AddUser("bob");
        bob.Login = "alice";

        Func<Task> act = () => _repo.UpdateAsync(bob);

        Assert.ThrowsAsync<DuplicateLoginException>(act);
    }

    [Test]
    public void Add_OtherConstraintViolation_IsNotMaskedAsDuplicate()
    {
        // NOT NULL тоже ошибка SQLite, но не про уникальность: она должна дойти до вызывающего кода как есть
        var broken = new User { Login = null!, PassHash = "h" };

        Func<Task> act = () => _repo.AddAsync(broken);

        Assert.ThrowsAsync<DbUpdateException>(act);
    }

    [Test]
    public void IsUniqueViolation_RecognizesOnlyUniqueConstraint()
    {
        Assert.That(UserRepository.IsUniqueViolation(new DbUpdateException("x", new SqliteException("unique", 19, 2067))), Is.True);
        Assert.That(UserRepository.IsUniqueViolation(new DbUpdateException("x", new SqliteException("not null", 19, 1299))), Is.False);
        Assert.That(UserRepository.IsUniqueViolation(new DbUpdateException("x", new InvalidOperationException())), Is.False);
        Assert.That(UserRepository.IsUniqueViolation(new DbUpdateException("x")), Is.False);
    }
}
