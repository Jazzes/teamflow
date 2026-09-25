using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Users.Core.Contracts;
using Users.Core.Security;
using Users.Core.Services;
using Users.Core.Validation;
using Users.Data;

// Простой замер: прогрев, затем N повторов, результат в миллисекундах на одну операцию.
// Если задана переменная GITHUB_STEP_SUMMARY, таблица дописывается в сводку запуска GitHub Actions.
Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
const string hash = "3875034e17855bac03a3cc9e107b1d28a9b44313d381c3335588525b4e70b55b";

var rows = new List<(string Name, int Count, double MsPerOp)>();

void Measure(string name, int count, Action action)
{
    for (var i = 0; i < Math.Min(count, 5); i++)
    {
        action();
    }

    var sw = Stopwatch.StartNew();
    for (var i = 0; i < count; i++)
    {
        action();
    }

    sw.Stop();
    rows.Add((name, count, sw.Elapsed.TotalMilliseconds / count));
}

var validator = new UserRequestValidator();
var request = new UserRequest("alice", hash);
Measure("Валидация запроса", 100_000, () => validator.Validate(request));

var salts = new RandomSaltGenerator();
foreach (var iterations in new[] { 100_000, 10_000, 1_000 })
{
    var protector = new Pbkdf2PasswordHashProtector(salts, new Pbkdf2Options(iterations));
    var stored = protector.Protect(hash);
    var count = iterations >= 100_000 ? 20 : 200;
    Measure($"PBKDF2 Protect, {iterations:N0} итераций", count, () => protector.Protect(hash));
    Measure($"PBKDF2 Verify, {iterations:N0} итераций", count, () => protector.Verify(hash, stored));
}

// Полный сценарий создания пользователя через сервис и SQLite в памяти
using var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
using var db = new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseSqlite(connection).Options);
db.Database.EnsureCreated();
var service = new UserService(new UserRepository(db), validator, new Pbkdf2PasswordHashProtector(salts, new Pbkdf2Options(1_000)));
var n = 0;
Measure("UserService.Create + SQLite (1 000 итераций PBKDF2)", 500,
    () => service.CreateAsync(new UserRequest($"user{n++:D6}", hash)).GetAwaiter().GetResult());

var table = new StringBuilder();
table.AppendLine("| Операция | Повторов | мс на операцию |");
table.AppendLine("|---|---:|---:|");
foreach (var (name, count, ms) in rows)
{
    table.AppendLine($"| {name} | {count} | {ms:F4} |");
}

Console.WriteLine($"{"Операция",-52} {"Повторов",9} {"мс/оп",10}");
foreach (var (name, count, ms) in rows)
{
    Console.WriteLine($"{name,-52} {count,9} {ms,10:F4}");
}

var summary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
if (!string.IsNullOrEmpty(summary))
{
    File.AppendAllText(summary, "## Замеры производительности\n\n" + table + "\n");
}
