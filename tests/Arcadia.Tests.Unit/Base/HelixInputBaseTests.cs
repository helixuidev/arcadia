using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace Arcadia.Tests.Unit.Base;

/// <summary>
/// A minimal concrete input component for testing HelixInputBase.
/// </summary>
public class TestInput : Arcadia.Core.Base.HelixInputBase<string>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "id", ElementId);
        builder.AddAttribute(2, "class", Class);
        builder.AddAttribute(3, "value", Value);
        builder.AddAttribute(4, "disabled", Disabled);
        builder.AddAttribute(5, "readonly", ReadOnly);
        builder.AddAttribute(6, "onchange",
            EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
                CurrentValue = e.Value?.ToString()));
        builder.AddMultipleAttributes(7, AdditionalAttributes!);
        builder.CloseElement();
    }
}

/// <summary>
/// A numeric input for testing value type binding.
/// </summary>
public class TestNumericInput : Arcadia.Core.Base.HelixInputBase<int>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "number");
        builder.AddAttribute(2, "value", Value);
        builder.AddAttribute(3, "onchange",
            EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
            {
                if (int.TryParse(e.Value?.ToString(), out var val))
                    CurrentValue = val;
            }));
        builder.CloseElement();
    }
}

public class HelixInputBaseTests : BunitContext
{
    [Fact]
    public void Renders_WithValue()
    {
        var cut = Render<TestInput>(parameters =>
            parameters.Add(p => p.Value, "hello"));

        cut.Find("input").GetAttribute("value").Should().Be("hello");
    }

    [Fact]
    public void Renders_WithDisabled()
    {
        var cut = Render<TestInput>(parameters =>
            parameters.Add(p => p.Disabled, true));

        cut.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Renders_WithReadOnly()
    {
        var cut = Render<TestInput>(parameters =>
            parameters.Add(p => p.ReadOnly, true));

        cut.Find("input").HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void ValueChanged_Fires_OnChange()
    {
        string? newValue = null;

        var cut = Render<TestInput>(parameters =>
            parameters.Add(p => p.Value, "initial")
                      .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => newValue = v)));

        cut.Find("input").Change("updated");

        newValue.Should().Be("updated");
    }

    [Fact]
    public void ValueChanged_DoesNotFire_WhenSameValue()
    {
        var fireCount = 0;

        var cut = Render<TestInput>(parameters =>
            parameters.Add(p => p.Value, "same")
                      .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, _ => fireCount++)));

        cut.Find("input").Change("same");

        fireCount.Should().Be(0);
    }

    [Fact]
    public void ImplementsIHasDisabled()
    {
        var cut = Render<TestInput>();

        cut.Instance.Should().BeAssignableTo<Arcadia.Core.Abstractions.IHasDisabled>();
    }

    [Fact]
    public void InheritsHelixComponentBase()
    {
        var cut = Render<TestInput>(parameters =>
            parameters.Add(p => p.Class, "custom"));

        cut.Find("input").GetAttribute("class").Should().Be("custom");
    }

    [Fact]
    public void NumericInput_ValueChanged_Fires()
    {
        int? newValue = null;

        var cut = Render<TestNumericInput>(parameters =>
            parameters.Add(p => p.Value, 0)
                      .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => newValue = v)));

        cut.Find("input").Change("42");

        newValue.Should().Be(42);
    }
}
