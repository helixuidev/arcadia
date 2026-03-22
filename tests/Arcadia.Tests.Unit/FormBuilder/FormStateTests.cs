using FluentAssertions;
using Arcadia.FormBuilder;
using Arcadia.FormBuilder.Schema;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class FormStateTests
{
    [Fact]
    public void SetValue_StoresValue()
    {
        var state = new FormState();
        state.SetValue("name", "John");

        state.GetValue("name").Should().Be("John");
    }

    [Fact]
    public void GetValue_Typed_ReturnsTyped()
    {
        var state = new FormState();
        state.SetValue("age", 25);

        state.GetValue<int>("age").Should().Be(25);
    }

    [Fact]
    public void GetValue_Missing_ReturnsNull()
    {
        var state = new FormState();

        state.GetValue("missing").Should().BeNull();
    }

    [Fact]
    public void SetValue_FiresOnValuesChanged()
    {
        var state = new FormState();
        var fired = false;
        state.OnValuesChanged += () => fired = true;

        state.SetValue("name", "John");

        fired.Should().BeTrue();
    }

    [Fact]
    public void SetErrors_StoresErrors()
    {
        var state = new FormState();
        state.SetErrors("email", new List<string> { "Email is required" });

        state.GetErrors("email").Should().ContainSingle("Email is required");
        state.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void SetErrors_Empty_RemovesErrors()
    {
        var state = new FormState();
        state.SetErrors("email", new List<string> { "Error" });
        state.SetErrors("email", new List<string>());

        state.GetErrors("email").Should().BeEmpty();
        state.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void ClearErrors_RemovesAll()
    {
        var state = new FormState();
        state.SetErrors("a", new List<string> { "Error A" });
        state.SetErrors("b", new List<string> { "Error B" });

        state.ClearErrors();

        state.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void MarkSubmitted_SetsFlag()
    {
        var state = new FormState();

        state.IsSubmitted.Should().BeFalse();
        state.MarkSubmitted();
        state.IsSubmitted.Should().BeTrue();
    }

    [Fact]
    public void InitializeDefaults_SetsDefaultValues()
    {
        var state = new FormState();
        var schema = new FormSchema
        {
            Fields = new List<FieldSchema>
            {
                new() { Name = "country", DefaultValue = "US" },
                new() { Name = "name" }
            }
        };

        state.InitializeDefaults(schema);

        state.GetValue("country").Should().Be("US");
        state.GetValue("name").Should().BeNull();
    }

    [Fact]
    public void InitializeDefaults_DoesNotOverwriteExisting()
    {
        var state = new FormState();
        state.SetValue("country", "UK");
        var schema = new FormSchema
        {
            Fields = new List<FieldSchema>
            {
                new() { Name = "country", DefaultValue = "US" }
            }
        };

        state.InitializeDefaults(schema);

        state.GetValue("country").Should().Be("UK");
    }
}
