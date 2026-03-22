using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Arcadia.FormBuilder.ModelBinding;
using Arcadia.FormBuilder.Schema;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

// Test models
public class SimpleModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class AnnotatedModel
{
    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Full Name", Prompt = "Enter your name", Description = "Your legal name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Range(18, 120)]
    public int Age { get; set; }

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.MultilineText)]
    public string Bio { get; set; } = string.Empty;
}

public enum Priority { Low, Medium, High, Critical }

public class EnumModel
{
    public Priority Priority { get; set; }
    public Priority? OptionalPriority { get; set; }
}

public class SectionedModel
{
    [HelixSection("Personal", Order = 0)]
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [HelixSection("Personal", Order = 0)]
    public string LastName { get; set; } = string.Empty;

    [HelixSection("Contact", Order = 1, Description = "How to reach you")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [HelixSection("Contact", Order = 1)]
    [Phone]
    public string Phone { get; set; } = string.Empty;
}

public class AttributeModel
{
    [HelixField(Placeholder = "jane@example.com", HelperText = "We won't share this", ColumnSpan = 6, Order = 1)]
    public string Email { get; set; } = string.Empty;

    [HelixField(Type = FieldType.TextArea, Order = 2)]
    public string Notes { get; set; } = string.Empty;

    [HelixIgnore]
    public string InternalId { get; set; } = string.Empty;
}

public class ConditionalModel
{
    public string ContactMethod { get; set; } = string.Empty;

    [HelixCondition("ContactMethod", Equals = "Phone")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ModelFormGeneratorTests
{
    [Fact]
    public void Generate_SimpleModel_InfersFieldTypes()
    {
        var schema = ModelFormGenerator.Generate<SimpleModel>();

        schema.Fields.Should().HaveCount(4);
        schema.Fields.First(f => f.Name == "Name").Type.Should().Be(FieldType.Text);
        schema.Fields.First(f => f.Name == "Age").Type.Should().Be(FieldType.Number);
        schema.Fields.First(f => f.Name == "IsActive").Type.Should().Be(FieldType.Checkbox);
        schema.Fields.First(f => f.Name == "BirthDate").Type.Should().Be(FieldType.Date);
    }

    [Fact]
    public void Generate_SimpleModel_DetectsRequired()
    {
        var schema = ModelFormGenerator.Generate<SimpleModel>();

        schema.Fields.First(f => f.Name == "Name").Required.Should().BeTrue();
        schema.Fields.First(f => f.Name == "Age").Required.Should().BeFalse();
    }

    [Fact]
    public void Generate_AnnotatedModel_UsesDisplayAttributes()
    {
        var schema = ModelFormGenerator.Generate<AnnotatedModel>();

        var nameField = schema.Fields.First(f => f.Name == "FullName");
        nameField.Label.Should().Be("Full Name");
        nameField.Placeholder.Should().Be("Enter your name");
        nameField.HelperText.Should().Be("Your legal name");
    }

    [Fact]
    public void Generate_AnnotatedModel_ExtractsValidation()
    {
        var schema = ModelFormGenerator.Generate<AnnotatedModel>();

        var nameField = schema.Fields.First(f => f.Name == "FullName");
        nameField.Validation.Should().NotBeNull();
        nameField.Validation!.MinLength.Should().Be(2);
        nameField.Validation!.MaxLength.Should().Be(100);

        var ageField = schema.Fields.First(f => f.Name == "Age");
        ageField.Validation!.Min.Should().Be(18);
        ageField.Validation!.Max.Should().Be(120);

        var emailField = schema.Fields.First(f => f.Name == "Email");
        emailField.Validation!.Pattern.Should().Be("email");
    }

    [Fact]
    public void Generate_AnnotatedModel_UsesDataType()
    {
        var schema = ModelFormGenerator.Generate<AnnotatedModel>();

        schema.Fields.First(f => f.Name == "Password").Type.Should().Be(FieldType.Password);
        schema.Fields.First(f => f.Name == "Bio").Type.Should().Be(FieldType.TextArea);
    }

    [Fact]
    public void Generate_EnumModel_CreatesSelectWithOptions()
    {
        var schema = ModelFormGenerator.Generate<EnumModel>();

        var field = schema.Fields.First(f => f.Name == "Priority");
        field.Type.Should().Be(FieldType.Select);
        field.Options.Should().HaveCount(4);
        field.Options![0].Value.Should().Be("Low");
        field.Options![3].Value.Should().Be("Critical");
    }

    [Fact]
    public void Generate_EnumModel_NullableEnum()
    {
        var schema = ModelFormGenerator.Generate<EnumModel>();

        var field = schema.Fields.First(f => f.Name == "OptionalPriority");
        field.Type.Should().Be(FieldType.Select);
        field.Options.Should().HaveCount(4);
    }

    [Fact]
    public void Generate_SectionedModel_GroupsIntoSections()
    {
        var schema = ModelFormGenerator.Generate<SectionedModel>();

        schema.Fields.Should().BeEmpty(); // All fields are in sections
        schema.Sections.Should().HaveCount(2);
        schema.Sections[0].Title.Should().Be("Personal");
        schema.Sections[0].Fields.Should().HaveCount(2);
        schema.Sections[1].Title.Should().Be("Contact");
        schema.Sections[1].Fields.Should().HaveCount(2);
        schema.Sections[1].Description.Should().Be("How to reach you");
    }

    [Fact]
    public void Generate_AttributeModel_UsesHelixFieldAttributes()
    {
        var schema = ModelFormGenerator.Generate<AttributeModel>();

        var emailField = schema.Fields.First(f => f.Name == "Email");
        emailField.Placeholder.Should().Be("jane@example.com");
        emailField.HelperText.Should().Be("We won't share this");
        emailField.ColumnSpan.Should().Be(6);

        var notesField = schema.Fields.First(f => f.Name == "Notes");
        notesField.Type.Should().Be(FieldType.TextArea);
    }

    [Fact]
    public void Generate_AttributeModel_IgnoresMarkedProperties()
    {
        var schema = ModelFormGenerator.Generate<AttributeModel>();

        schema.Fields.Should().NotContain(f => f.Name == "InternalId");
        schema.Fields.Should().HaveCount(2);
    }

    [Fact]
    public void Generate_AttributeModel_RespectsOrder()
    {
        var schema = ModelFormGenerator.Generate<AttributeModel>();

        schema.Fields[0].Name.Should().Be("Email");
        schema.Fields[1].Name.Should().Be("Notes");
    }

    [Fact]
    public void Generate_ConditionalModel_CreatesConditions()
    {
        var schema = ModelFormGenerator.Generate<ConditionalModel>();

        var phoneField = schema.Fields.First(f => f.Name == "PhoneNumber");
        phoneField.Conditions.Should().HaveCount(1);
        phoneField.Conditions![0].Field.Should().Be("ContactMethod");
        phoneField.Conditions![0].Operator.Should().Be(ConditionalOperator.Equals);
        phoneField.Conditions![0].Value.Should().Be("Phone");
    }

    [Fact]
    public void Generate_CustomTitle()
    {
        var schema = ModelFormGenerator.Generate<SimpleModel>("My Custom Form");

        schema.Title.Should().Be("My Custom Form");
    }

    [Fact]
    public void Generate_DefaultTitle_HumanizedTypeName()
    {
        var schema = ModelFormGenerator.Generate<SimpleModel>();

        schema.Title.Should().Be("Simple Model");
    }
}
