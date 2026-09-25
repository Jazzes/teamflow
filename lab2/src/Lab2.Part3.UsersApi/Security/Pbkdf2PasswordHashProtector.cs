using System.Security.Cryptography;
using System.Text;

namespace Lab2.Part3.UsersApi.Security;

/// <summary>
/// Хранит хеш в формате pbkdf2-sha256$итерации$соль$хеш.
/// Если база утечёт, по сохранённой строке нельзя войти: сервер ждёт исходный SHA-256,
/// а восстановить его из PBKDF2 с солью практически невозможно.
/// </summary>
public class Pbkdf2PasswordHashProtector : IPasswordHashProtector
{
    private const string Prefix = "pbkdf2-sha256";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string Protect(string clientHash)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(clientHash, salt, Iterations);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string clientHash, string storedValue)
    {
        var parts = storedValue.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Derive(clientHash, salt, iterations);

        // Сравнение за постоянное время, чтобы по времени ответа нельзя было подбирать хеш
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Derive(string clientHash, byte[] salt, int iterations)
    {
        // Регистр hex-строки не должен влиять на результат
        var input = Encoding.UTF8.GetBytes(clientHash.ToLowerInvariant());
        return Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, HashAlgorithmName.SHA256, HashSize);
    }
}
