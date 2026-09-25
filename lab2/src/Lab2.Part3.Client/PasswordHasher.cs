using System.Security.Cryptography;
using System.Text;

namespace Lab2.Part3.Client;

/// <summary>
/// Хеширование пароля на стороне клиента: по сети уходит только SHA-256, а не сам пароль.
/// </summary>
public static class PasswordHasher
{
    public static string Sha256Hex(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
