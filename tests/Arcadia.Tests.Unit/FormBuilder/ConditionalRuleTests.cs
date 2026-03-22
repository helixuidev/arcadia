using FluentAssertions;
using Arcadia.FormBuilder.Schema;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class ConditionalRuleTests
{
    [Fact]
    public void Equals_MatchingValue_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "type", Operator = ConditionalOperator.Equals, Value = "phone" };
        var values = new Dictionary<string, object?> { ["type"] = "phone" };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void Equals_NonMatchingValue_ReturnsFalse()
    {
        var rule = new ConditionalRule { Field = "type", Operator = ConditionalOperator.Equals, Value = "phone" };
        var values = new Dictionary<string, object?> { ["type"] = "email" };

        rule.Evaluate(values).Should().BeFalse();
    }

    [Fact]
    public void NotEquals_DifferentValue_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "type", Operator = ConditionalOperator.NotEquals, Value = "phone" };
        var values = new Dictionary<string, object?> { ["type"] = "email" };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void Contains_SubstringPresent_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "name", Operator = ConditionalOperator.Contains, Value = "john" };
        var values = new Dictionary<string, object?> { ["name"] = "John Smith" };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_NullValue_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "name", Operator = ConditionalOperator.IsEmpty };
        var values = new Dictionary<string, object?> { ["name"] = null };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void IsNotEmpty_WithValue_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "name", Operator = ConditionalOperator.IsNotEmpty };
        var values = new Dictionary<string, object?> { ["name"] = "John" };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_LargerValue_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "age", Operator = ConditionalOperator.GreaterThan, Value = 18 };
        var values = new Dictionary<string, object?> { ["age"] = "25" };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void LessThan_SmallerValue_ReturnsTrue()
    {
        var rule = new ConditionalRule { Field = "age", Operator = ConditionalOperator.LessThan, Value = 18 };
        var values = new Dictionary<string, object?> { ["age"] = "10" };

        rule.Evaluate(values).Should().BeTrue();
    }

    [Fact]
    public void MissingField_ReturnsFalse()
    {
        var rule = new ConditionalRule { Field = "missing", Operator = ConditionalOperator.Equals, Value = "x" };
        var values = new Dictionary<string, object?>();

        rule.Evaluate(values).Should().BeFalse();
    }
}
