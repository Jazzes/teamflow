using System.Security.Cryptography;
using System.Text;
using Users.Core.Security;

namespace Users.Tests.Core;

[TestFixture]
public class Pbkdf2PasswordHashProtectorTests
{
    private static readonly byte[] FixedSalt = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();

    private ISaltGenerator _salts = null!;
    private Pbkdf2PasswordHashProtector _protector = null!;

    [SetUp]
    public void SetUp()
    {
        _salts = Substitute.For<ISaltGenerator>();
        _salts.Generate(Arg.Any<int>()).Returns(FixedSalt);
        // В тестах минимальное число итераций: проверяется логика, а не стойкость
        _protector = new Pbkdf2PasswordHashProtector(_salts, new Pbkdf2Options(Pbkdf2Options.MinIterations));
    }

    [Test]
    public void Protect_ThenVerify_SamePassword_ReturnsTrue()
    {
        var stored = _protector.Protect(TestData.Hash);

        Assert.That(_protector.Verify(TestData.Hash, stored), Is.True);
    }

    [Test]
    public void Protect_UsesSaltFromGeneratorAndConfiguredIterations()
    {
        var stored = _protector.Protect(TestData.Hash);

        var expectedHash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(TestData.Hash), FixedSalt, 1000, HashAlgorithmName.SHA256, 32);
        Assert.That(stored, Is.EqualTo($"pbkdf2-sha256$1000${Convert.ToBase64String(FixedSalt)}${Convert.ToBase64String(expectedHash)}"));
        _salts.Received(1).Generate(16);
    }

    [Test]
    public void Protect_EmptyHash_Throws()
    {
        Action act = () => _protector.Protect("");

        Assert.Throws<ArgumentException>(act);
    }

    [Test]
    public void Verify_HexCaseDoesNotMatter()
    {
        var stored = _protector.Protect(TestData.Hash);

        Assert.That(_protector.Verify(TestData.Hash.ToUpperInvariant(), stored), Is.True);
    }

    [Test]
    public void Verify_OtherPassword_ReturnsFalse()
    {
        var stored = _protector.Protect(TestData.Hash);

        Assert.That(_protector.Verify(TestData.OtherHash, stored), Is.False);
    }

    [TestCase("", "pbkdf2-sha256$1000$AQID$AQID", TestName = "Пустой клиентский хеш")]
    [TestCase(TestData.Hash, "", TestName = "Пустая сохранённая строка")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$1000$AQID", TestName = "Не хватает частей")]
    [TestCase(TestData.Hash, "bcrypt$1000$AQID$AQID", TestName = "Чужой префикс")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$abc$AQID$AQID", TestName = "Число итераций не число")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$0$AQID$AQID", TestName = "Нулевое число итераций")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$1000$@@@$AQID", TestName = "Соль не Base64")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$1000$AQID$%%%", TestName = "Хеш не Base64")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$1000$$AQID", TestName = "Пустая соль")]
    [TestCase(TestData.Hash, "pbkdf2-sha256$1000$AQID$", TestName = "Пустой хеш")]
    public void Verify_CorruptedOrForeignValue_ReturnsFalseInsteadOfThrowing(string clientHash, string stored)
    {
        Assert.That(_protector.Verify(clientHash, stored), Is.False);
    }
}

[TestFixture]
public class Pbkdf2OptionsTests
{
    [Test]
    public void Defaults_AreSafeForProduction()
    {
        var options = new Pbkdf2Options();

        Assert.That(options.Iterations, Is.EqualTo(100_000));
        Assert.That(options.SaltSize, Is.EqualTo(16));
        Assert.That(options.HashSize, Is.EqualTo(32));
    }

    [TestCase(999, 16, 32)]
    [TestCase(1000, 7, 32)]
    [TestCase(1000, 16, 15)]
    public void Constructor_UnsafeValues_Throw(int iterations, int saltSize, int hashSize)
    {
        Action act = () => _ = new Pbkdf2Options(iterations, saltSize, hashSize);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

[TestFixture]
public class RandomSaltGeneratorTests
{
    private readonly RandomSaltGenerator _generator = new();

    [Test]
    public void Generate_ReturnsRequestedLengthAndDifferentValues()
    {
        var first = _generator.Generate(16);
        var second = _generator.Generate(16);

        Assert.That(first, Has.Length.EqualTo(16));
        Assert.That(first, Is.Not.EqualTo(second));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Generate_NonPositiveSize_Throws(int size)
    {
        Action act = () => _generator.Generate(size);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}
