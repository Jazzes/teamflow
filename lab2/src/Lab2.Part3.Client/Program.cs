using System.Text;
using Lab2.Part3.Client;

Console.OutputEncoding = Encoding.UTF8;

// Адрес сервера можно передать аргументом: dotnet run -- http://localhost:5080
var baseAddress = args.Length > 0 ? args[0] : "http://localhost:5080";
using var http = new HttpClient { BaseAddress = new Uri(baseAddress) };
var api = new UsersApiClient(http);

var alicePassword = PasswordHasher.Sha256Hex("Qwerty123!");
var aliceNewPassword = PasswordHasher.Sha256Hex("NewPass2026");
var bobPassword = PasswordHasher.Sha256Hex("b0b-secret");

Step("1. Создание двух пользователей");
var alice = await api.SendAsync(HttpMethod.Post, "/user", new { Login = "alice", PassHash = alicePassword });
await api.SendAsync(HttpMethod.Post, "/user", new { Login = "bob", PassHash = bobPassword });
var aliceId = alice?.GetProperty("Data").GetProperty("Id").GetInt32() ?? 1;

Step("2. Чтение пользователя по Id");
await api.SendAsync(HttpMethod.Get, $"/user/{aliceId}");

Step("3. Повторный логин (регистр не важен) и открытый пароль вместо хеша");
await api.SendAsync(HttpMethod.Post, "/user", new { Login = "ALICE", PassHash = alicePassword });
await api.SendAsync(HttpMethod.Post, "/user", new { Login = "carol", PassHash = "Qwerty123!" });

Step("4. Обновление логина и пароля");
await api.SendAsync(HttpMethod.Put, $"/user/{aliceId}", new { Login = "alice_new", PassHash = aliceNewPassword });

Step("5. Проверка пароля: старый и новый");
await api.SendAsync(HttpMethod.Post, "/user/verify", new { Login = "alice_new", PassHash = alicePassword });
await api.SendAsync(HttpMethod.Post, "/user/verify", new { Login = "alice_new", PassHash = aliceNewPassword });

Step("6. Удаление пользователя и попытка прочитать удалённого");
await api.SendAsync(HttpMethod.Delete, "/user/2");
await api.SendAsync(HttpMethod.Get, "/user/2");

Step("7. Итоговый список");
await api.SendAsync(HttpMethod.Get, "/user");

static void Step(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}
