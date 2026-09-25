using Users.Core.Validation;

namespace Users.Tests.Core;

[TestFixture]
public class UserRequestValidatorTests
{
    private readonly UserRequestValidator _validator = new();

    [Test]
    public void Validate_CorrectRequest_ReturnsNoErrors()
    {
        var errors = _validator.Validate(TestData.Request());

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_NullRequest_ReturnsBodyError()
    {
        var errors = _validator.Validate(null);

        Assert.That(errors.Keys, Is.EquivalentTo(new[] { "Body" }));
    }

    [TestCase(null, "Логин обязателен.")]
    [TestCase("", "Логин обязателен.")]
    [TestCase("   ", "Логин обязателен.")]
    [TestCase("ab", "Длина логина от 3 до 32 символов.")]
    [TestCase("a_very_long_login_that_exceeds_32", "Длина логина от 3 до 32 символов.")]
    [TestCase("алиса", "Логин может содержать только латинские буквы, цифры, точку, дефис и подчёркивание.")]
    [TestCase("x'; DROP TABLE Users;--", "Логин может содержать только латинские буквы, цифры, точку, дефис и подчёркивание.")]
    public void Validate_BadLogin_ReturnsLoginError(string? login, string expected)
    {
        var errors = _validator.Validate(TestData.Request(login));

        Assert.That(errors["Login"], Is.EqualTo(new[] { expected }));
        Assert.That(errors.ContainsKey("PassHash"), Is.False);
    }

    [TestCase("abc")]
    [TestCase("user.name-01")]
    [TestCase("  padded_login  ")]
    public void Validate_GoodLogin_IsAccepted(string login)
    {
        Assert.That(_validator.Validate(TestData.Request(login)), Is.Empty);
    }

    [TestCase(null, "Хеш пароля обязателен.")]
    [TestCase("", "Хеш пароля обязателен.")]
    [TestCase("Qwerty123!", "Ожидается хеш SHA-256 в виде 64 шестнадцатеричных символов. Открытый пароль не принимается.")]
    [TestCase("3875034e17855bac03a3cc9e107b1d28a9b44313d381c3335588525b4e70b55", "Ожидается хеш SHA-256 в виде 64 шестнадцатеричных символов. Открытый пароль не принимается.")]
    public void Validate_BadHash_ReturnsHashError(string? hash, string expected)
    {
        var errors = _validator.Validate(TestData.Request(hash: hash));

        Assert.That(errors["PassHash"], Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Validate_UppercaseHash_IsAccepted()
    {
        Assert.That(_validator.Validate(TestData.Request(hash: TestData.Hash.ToUpperInvariant())), Is.Empty);
    }

    [Test]
    public void Validate_BothFieldsWrong_ReturnsBothErrors()
    {
        var errors = _validator.Validate(TestData.Request("", "123"));

        Assert.That(errors.Keys, Is.EquivalentTo(new[] { "Login", "PassHash" }));
    }
}
