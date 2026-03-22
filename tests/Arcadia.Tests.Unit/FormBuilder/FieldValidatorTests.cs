using FluentAssertions;
using Arcadia.FormBuilder.Schema;
using Arcadia.FormBuilder.Validation;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class FieldValidatorTests
{
    [Fact]
    public void Required_Empty_ReturnsError()
    {
        var field = new FieldSchema { Name = "name", Label = "Name", Required = true };

        var errors = FieldValidator.Validate(field, null);

        errors.Should().ContainSingle().Which.Should().Contain("required");
    }

    [Fact]
    public void Required_WithValue_NoError()
    {
        var field = new FieldSchema { Name = "name", Label = "Name", Required = true };

        var errors = FieldValidator.Validate(field, "John");

        errors.Should().BeEmpty();
    }

    [Fact]
    public void NotRequired_Empty_NoError()
    {
        var field = new FieldSchema { Name = "name", Label = "Name", Required = false };

        var errors = FieldValidator.Validate(field, null);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void MinLength_TooShort_ReturnsError()
    {
        var field = new FieldSchema
        {
            Name = "name", Label = "Name",
            Validation = new ValidationRule { MinLength = 3 }
        };

        var errors = FieldValidator.Validate(field, "ab");

        errors.Should().ContainSingle().Which.Should().Contain("at least 3");
    }

    [Fact]
    public void MaxLength_TooLong_ReturnsError()
    {
        var field = new FieldSchema
        {
            Name = "name", Label = "Name",
            Validation = new ValidationRule { MaxLength = 5 }
        };

        var errors = FieldValidator.Validate(field, "toolongvalue");

        errors.Should().ContainSingle().Which.Should().Contain("at most 5");
    }

    [Fact]
    public void Min_BelowMinimum_ReturnsError()
    {
        var field = new FieldSchema
        {
            Name = "age", Label = "Age",
            Validation = new ValidationRule { Min = 18 }
        };

        var errors = FieldValidator.Validate(field, "15");

        errors.Should().ContainSingle().Which.Should().Contain("at least 18");
    }

    [Fact]
    public void Max_AboveMaximum_ReturnsError()
    {
        var field = new FieldSchema
        {
            Name = "age", Label = "Age",
            Validation = new ValidationRule { Max = 100 }
        };

        var errors = FieldValidator.Validate(field, "150");

        errors.Should().ContainSingle().Which.Should().Contain("at most 100");
    }

    [Fact]
    public void Pattern_Email_Valid_NoError()
    {
        var field = new FieldSchema
        {
            Name = "email", Label = "Email",
            Validation = new ValidationRule { Pattern = "email" }
        };

        var errors = FieldValidator.Validate(field, "test@example.com");

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Pattern_Email_Invalid_ReturnsError()
    {
        var field = new FieldSchema
        {
            Name = "email", Label = "Email",
            Validation = new ValidationRule { Pattern = "email" }
        };

        var errors = FieldValidator.Validate(field, "not-an-email");

        errors.Should().ContainSingle().Which.Should().Contain("valid format");
    }

    [Fact]
    public void CustomMessage_UsedInError()
    {
        var field = new FieldSchema
        {
            Name = "email", Label = "Email",
            Validation = new ValidationRule { Pattern = "email", Message = "Enter a valid email address" }
        };

        var errors = FieldValidator.Validate(field, "bad");

        errors.Should().ContainSingle("Enter a valid email address");
    }

    [Fact]
    public void NoValidation_NoErrors()
    {
        var field = new FieldSchema { Name = "notes", Label = "Notes" };

        var errors = FieldValidator.Validate(field, "anything");

        errors.Should().BeEmpty();
    }
}
