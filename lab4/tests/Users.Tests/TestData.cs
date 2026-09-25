using Users.Core.Contracts;

namespace Users.Tests;

/// <summary>
/// Общие данные для тестов.
/// </summary>
internal static class TestData
{
    /// <summary>SHA-256 от строки «Qwerty123!» в hex.</summary>
    public const string Hash = "3875034e17855bac03a3cc9e107b1d28a9b44313d381c3335588525b4e70b55b";

    /// <summary>SHA-256 от строки «NewPass2026» в hex.</summary>
    public const string OtherHash = "10377176f45381233953769b58d5589eaed878fc05b327bd279873ca316b6d33";

    public static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    public static UserRequest Request(string? login = "alice", string? hash = Hash) => new(login, hash);
}
