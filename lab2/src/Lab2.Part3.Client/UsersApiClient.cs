using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Lab2.Part3.Client;

/// <summary>
/// Тонкая обёртка над HttpClient: отправляет JSON и печатает запрос и ответ.
/// </summary>
public class UsersApiClient
{
    private static readonly JsonSerializerOptions PrettyJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.Cyrillic),
    };

    private readonly HttpClient _http;

    public UsersApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<JsonElement?> SendAsync(HttpMethod method, string path, object? body = null)
    {
        return SendCoreAsync(method, path, body);
    }

    private async Task<JsonElement?> SendCoreAsync(HttpMethod method, string path, object? body)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine($"-> {method} {path}  {ShortenHashes(json)}");
        }
        else
        {
            Console.WriteLine($"-> {method} {path}");
        }

        using var response = await _http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"<- {(int)response.StatusCode} {response.StatusCode}");

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement.Clone();
        foreach (var line in JsonSerializer.Serialize(root, PrettyJson).Split('\n'))
        {
            Console.WriteLine($"   {line.TrimEnd()}");
        }

        return root;
    }

    /// <summary>Для читаемости вывода длинный хеш сокращается до первых 12 символов.</summary>
    private static string ShortenHashes(string json)
    {
        using var document = JsonDocument.Parse(json);
        var parts = new List<string>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var value = property.Value.ToString();
            if (property.Name == "PassHash" && value.Length > 16)
            {
                value = value[..12] + "...";
            }

            parts.Add($"\"{property.Name}\":\"{value}\"");
        }

        return "{" + string.Join(",", parts) + "}";
    }
}
