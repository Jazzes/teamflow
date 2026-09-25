namespace Users.Core.Security;

/// <summary>
/// Параметры PBKDF2. Раньше число итераций было константой в коде,
/// теперь его можно задать в конфигурации и уменьшить в тестах.
/// </summary>
public class Pbkdf2Options
{
    /// <summary>Минимум, ниже которого хранить пароли небезопасно даже в тестовом окружении.</summary>
    public const int MinIterations = 1_000;

    public int Iterations { get; }

    public int SaltSize { get; }

    public int HashSize { get; }

    public Pbkdf2Options(int iterations = 100_000, int saltSize = 16, int hashSize = 32)
    {
        if (iterations < MinIterations)
        {
            throw new ArgumentOutOfRangeException(nameof(iterations), $"Число итераций не может быть меньше {MinIterations}.");
        }

        if (saltSize < 8 || hashSize < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(saltSize), "Соль не короче 8 байт, хеш не короче 16 байт.");
        }

        Iterations = iterations;
        SaltSize = saltSize;
        HashSize = hashSize;
    }
}
