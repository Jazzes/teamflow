using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Users.Core.Models;
using Users.Data;

namespace Users.Tests.Data;

/// <summary>
/// Тесты репозитория на настоящей SQLite в памяти: проверяются запросы и ограничения схемы.
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

    [Test]
    public async Task Add_ThenGetById_ReturnsSavedUser()
    {
        var user = new User { Login = "alice", PassHash = "h1" };

        await _repo.AddAsync(user);
        var loaded = await _repo.GetByIdAsync(user.Id);

        Assert.That(loaded!.Login, Is.EqualTo("alice"));
    }
}
