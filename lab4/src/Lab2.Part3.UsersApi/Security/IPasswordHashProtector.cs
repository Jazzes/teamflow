namespace Lab2.Part3.UsersApi.Security;

/// <summary>
/// Защищает присланный клиентом хеш перед записью в базу.
/// </summary>
public interface IPasswordHashProtector
{
    /// <summary>Превращает клиентский хеш в строку для хранения (соль + PBKDF2).</summary>
    string Protect(string clientHash);

    /// <summary>Сравнивает клиентский хеш с сохранённой строкой.</summary>
    bool Verify(string clientHash, string storedValue);
}
