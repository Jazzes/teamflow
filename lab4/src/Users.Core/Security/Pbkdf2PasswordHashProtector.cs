using System.Security.Cryptography;
using System.Text;

namespace Users.Core.Security;

/// <summary>
/// Хранит хеш в формате pbkdf2-sha256$итерации$соль$хеш.
/// Если база утечёт, по сохранённой строке нельзя войти: сервер ждёт исходный SHA-256,
/// а восстановить его из PBKDF2 с солью практически невозможно.
/// </summary>
public class Pbkdf2PasswordHashProtector : IPasswordHashProtector
{
    private const string Prefix = "pbkdf2-sha256";

    private readonly ISaltGenerator _salts;
    private readonly Pbkdf2Options _options;

    public Pbkdf2PasswordHashProtector(ISaltGenerator salts, Pbkdf2Options options)
    {
        _salts = salts;
        _options = options;
    }

    public string Protect(string clientHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientHash);

        var salt = _salts.Generate(_options.SaltSize);
        var hash = Derive(clientHash, salt, _options.Iterations, _options.HashSize);
        return $"{Prefix}${_options.Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string clientHash, string storedValue)
    {
        if (string.IsNullOrEmpty(clientHash) || string.IsNullOrEmpty(storedValue))
        {
            return false;
        }

        var parts = storedValue.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            if (salt.Length == 0 || expected.Length == 0)
            {
                return false;
            }

            var actual = Derive(clientHash, salt, iterations, expected.Length);

            // Сравнение за постоянное время, чтобы по времени ответа нельзя было подбирать хеш
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            // Повреждённая запись в базе: раньше здесь падал весь запрос с кодом 500
            return false;
        }
    }

    private static byte[] Derive(string clientHash, byte[] salt, int iterations, int size)
    {
        // Регистр hex-строки не должен влиять на результат
        var input = Encoding.UTF8.GetBytes(clientHash.ToLowerInvariant());
        return Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, HashAlgorithmName.SHA256, size);
    }
}
