using FluentAssertions;
using Arcadia.FormBuilder.Schema;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class SchemaParserTests
{
    [Fact]
    public void Parse_BasicForm_Deserializes()
    {
        var json = """
        {
            "title": "Test Form",
            "fields": [
                { "name": "name", "type": "text", "label": "Name", "required": true }
            ]
        }
        """;

        var schema = SchemaParser.Parse(json);

        schema.Title.Should().Be("Test Form");
        schema.Fields.Should().HaveCount(1);
        schema.Fields[0].Name.Should().Be("name");
        schema.Fields[0].Type.Should().Be(FieldType.Text);
        schema.Fields[0].Required.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithValidation_Deserializes()
    {
        var json = """
        {
            "fields": [
                {
                    "name": "email",
                    "type": "text",
                    "label": "Email",
                    "validation": { "pattern": "email", "minLength": 5 }
                }
            ]
        }
        """;

        var schema = SchemaParser.Parse(json);

        schema.Fields[0].Validation.Should().NotBeNull();
        schema.Fields[0].Validation!.Pattern.Should().Be("email");
        schema.Fields[0].Validation!.MinLength.Should().Be(5);
    }

    [Fact]
    public void Parse_WithSections_Deserializes()
    {
        var json = """
        {
            "sections": [
                {
                    "title": "Personal",
                    "fields": [
                        { "name": "firstName", "type": "text", "label": "First Name" }
                    ]
                }
            ]
        }
        """;

        var schema = SchemaParser.Parse(json);

        schema.Sections.Should().HaveCount(1);
        schema.Sections[0].Title.Should().Be("Personal");
        schema.Sections[0].Fields.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_WithConditions_Deserializes()
    {
        var json = """
        {
            "fields": [
                {
                    "name": "phone",
                    "type": "text",
                    "label": "Phone",
                    "conditions": [
                        { "field": "contactMethod", "operator": "equals", "value": "Phone", "action": "show" }
                    ]
                }
            ]
        }
        """;

        var schema = SchemaParser.Parse(json);

        schema.Fields[0].Conditions.Should().HaveCount(1);
        schema.Fields[0].Conditions![0].Field.Should().Be("contactMethod");
        schema.Fields[0].Conditions![0].Operator.Should().Be(ConditionalOperator.Equals);
        schema.Fields[0].Conditions![0].Action.Should().Be(ConditionalAction.Show);
    }

    [Fact]
    public void Parse_WithOptions_Deserializes()
    {
        var json = """
        {
            "fields": [
                {
                    "name": "color",
                    "type": "select",
                    "label": "Color",
                    "options": [
                        { "label": "Red", "value": "red" },
                        { "label": "Blue", "value": "blue" }
                    ]
                }
            ]
        }
        """;

        var schema = SchemaParser.Parse(json);

        schema.Fields[0].Options.Should().HaveCount(2);
        schema.Fields[0].Options![0].Label.Should().Be("Red");
        schema.Fields[0].Options![0].Value.Should().Be("red");
    }

    [Fact]
    public void ToJson_RoundTrips()
    {
        var schema = new FormSchema
        {
            Title = "Round Trip",
            Fields = new() { new() { Name = "test", Type = FieldType.Number, Label = "Test", Required = true } }
        };

        var json = SchemaParser.ToJson(schema);
        var parsed = SchemaParser.Parse(json);

        parsed.Title.Should().Be("Round Trip");
        parsed.Fields[0].Name.Should().Be("test");
        parsed.Fields[0].Type.Should().Be(FieldType.Number);
    }

    [Fact]
    public void TryParse_InvalidJson_ReturnsFalse()
    {
        var result = SchemaParser.TryParse("not json {{{", out var schema);

        result.Should().BeFalse();
        schema.Should().BeNull();
    }

    [Fact]
    public void TryParse_ValidJson_ReturnsTrue()
    {
        var result = SchemaParser.TryParse("""{ "title": "OK", "fields": [] }""", out var schema);

        result.Should().BeTrue();
        schema.Should().NotBeNull();
        schema!.Title.Should().Be("OK");
    }

    [Fact]
    public void Parse_AllFieldTypes()
    {
        var json = """
        {
            "fields": [
                { "name": "a", "type": "text", "label": "A" },
                { "name": "b", "type": "number", "label": "B" },
                { "name": "c", "type": "textArea", "label": "C" },
                { "name": "d", "type": "select", "label": "D" },
                { "name": "e", "type": "checkbox", "label": "E" },
                { "name": "f", "type": "radioGroup", "label": "F" },
                { "name": "g", "type": "date", "label": "G" },
                { "name": "h", "type": "switch", "label": "H" },
                { "name": "i", "type": "multiSelect", "label": "I" },
                { "name": "j", "type": "password", "label": "J" },
                { "name": "k", "type": "slider", "label": "K" },
                { "name": "l", "type": "rating", "label": "L" }
            ]
        }
        """;

        var schema = SchemaParser.Parse(json);

        schema.Fields.Should().HaveCount(12);
        schema.Fields[0].Type.Should().Be(FieldType.Text);
        schema.Fields[1].Type.Should().Be(FieldType.Number);
        schema.Fields[8].Type.Should().Be(FieldType.MultiSelect);
        schema.Fields[9].Type.Should().Be(FieldType.Password);
        schema.Fields[10].Type.Should().Be(FieldType.Slider);
        schema.Fields[11].Type.Should().Be(FieldType.Rating);
    }
}
