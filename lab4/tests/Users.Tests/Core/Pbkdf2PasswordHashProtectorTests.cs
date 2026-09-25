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
}
