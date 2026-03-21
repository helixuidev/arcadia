using FluentAssertions;
using HelixUI.FormBuilder.Validation;
using Xunit;

namespace HelixUI.Tests.Unit.FormBuilder;

public class CrossFieldValidatorTests
{
    [Fact]
    public void MustEqual_Matching_NoErrors()
    {
        var validator = new CrossFieldValidator()
            .MustEqual("confirmPassword", "password");

        var values = new Dictionary<string, object?>
        {
            ["password"] = "secret123",
            ["confirmPassword"] = "secret123"
        };

        var errors = validator.Validate("confirmPassword", "secret123", values);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void MustEqual_NotMatching_ReturnsError()
    {
        var validator = new CrossFieldValidator()
            .MustEqual("confirmPassword", "password", "Passwords must match");

        var values = new Dictionary<string, object?>
        {
            ["password"] = "secret123",
            ["confirmPassword"] = "different"
        };

        var errors = validator.Validate("confirmPassword", "different", values);

        errors.Should().ContainSingle("Passwords must match");
    }

    [Fact]
    public void MustBeAfter_DateAfter_NoErrors()
    {
        var validator = new CrossFieldValidator()
            .MustBeAfter("endDate", "startDate");

        var values = new Dictionary<string, object?>
        {
            ["startDate"] = "2026-01-01",
            ["endDate"] = "2026-12-31"
        };

        var errors = validator.Validate("endDate", "2026-12-31", values);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void MustBeAfter_DateBefore_ReturnsError()
    {
        var validator = new CrossFieldValidator()
            .MustBeAfter("endDate", "startDate");

        var values = new Dictionary<string, object?>
        {
            ["startDate"] = "2026-12-31",
            ["endDate"] = "2026-01-01"
        };

        var errors = validator.Validate("endDate", "2026-01-01", values);

        errors.Should().ContainSingle().Which.Should().Contain("after");
    }

    [Fact]
    public void AtLeastOneRequired_OneFilled_NoErrors()
    {
        var validator = new CrossFieldValidator()
            .AtLeastOneRequired(new[] { "email", "phone" });

        var values = new Dictionary<string, object?>
        {
            ["email"] = "test@example.com",
            ["phone"] = null
        };

        var errors = validator.Validate("email", "test@example.com", values);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void AtLeastOneRequired_NoneFilled_ReturnsError()
    {
        var validator = new CrossFieldValidator()
            .AtLeastOneRequired(new[] { "email", "phone" }, "Provide email or phone");

        var values = new Dictionary<string, object?>
        {
            ["email"] = null,
            ["phone"] = null
        };

        var errors = validator.Validate("email", null, values);

        errors.Should().ContainSingle("Provide email or phone");
    }

    [Fact]
    public void IrrelevantField_NoErrors()
    {
        var validator = new CrossFieldValidator()
            .MustEqual("confirmPassword", "password");

        var values = new Dictionary<string, object?>
        {
            ["name"] = "John"
        };

        var errors = validator.Validate("name", "John", values);

        errors.Should().BeEmpty();
    }
}
