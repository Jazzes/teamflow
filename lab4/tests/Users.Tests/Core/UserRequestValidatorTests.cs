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
}
