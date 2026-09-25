using System.Security.Cryptography;

namespace Users.Core.Security;

/// <summary>
/// Криптографически стойкая соль из системного генератора случайных чисел.
/// </summary>
public class RandomSaltGenerator : ISaltGenerator
{
    public byte[] Generate(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Размер соли должен быть положительным.");
        }

        return RandomNumberGenerator.GetBytes(size);
    }
}
