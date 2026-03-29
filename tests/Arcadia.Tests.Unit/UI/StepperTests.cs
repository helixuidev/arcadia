using Bunit;
using FluentAssertions;
using Xunit;
using Arcadia.UI.Components;
using Microsoft.AspNetCore.Components;

namespace Arcadia.Tests.Unit.UI;

public class StepperTests : ChartTestBase
{
    private RenderFragment ThreeSteps => builder =>
    {
        builder.OpenComponent<ArcadiaStep>(0);
        builder.AddAttribute(1, "Title", "Step 1");
        builder.CloseComponent();

        builder.OpenComponent<ArcadiaStep>(2);
        builder.AddAttribute(3, "Title", "Step 2");
        builder.CloseComponent();

        builder.OpenComponent<ArcadiaStep>(4);
        builder.AddAttribute(5, "Title", "Step 3");
        builder.CloseComponent();
    };

    [Fact]
    public void Renders_AllSteps()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ChildContent, ThreeSteps));

        cut.FindAll(".arcadia-step").Should().HaveCount(3);
    }

    [Fact]
    public void RoleList_OnContainer()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ChildContent, ThreeSteps));

        cut.Find("[role='list']").Should().NotBeNull();
    }

    [Fact]
    public void ActiveStep_HasActiveClass()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 1)
            .Add(c => c.ChildContent, ThreeSteps));

        cut.FindAll(".arcadia-step--active").Should().HaveCount(1);
    }

    [Fact]
    public void ActiveStep_HasAriaCurrentStep()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ChildContent, ThreeSteps));

        cut.Find("[aria-current='step']").Should().NotBeNull();
    }

    [Fact]
    public void CompletedStep_HasCompletedClass()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 2)
            .Add(c => c.ChildContent, ThreeSteps));

        cut.FindAll(".arcadia-step--completed").Should().HaveCount(2);
    }

    [Fact]
    public void CompletedStep_ShowsCheckmark()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 1)
            .Add(c => c.ChildContent, ThreeSteps));

        cut.FindAll(".arcadia-step__check").Should().HaveCount(1);
    }

    [Fact]
    public void DisabledStep_HasDisabledClass()
    {
        RenderFragment steps = builder =>
        {
            builder.OpenComponent<ArcadiaStep>(0);
            builder.AddAttribute(1, "Title", "Step 1");
            builder.CloseComponent();

            builder.OpenComponent<ArcadiaStep>(2);
            builder.AddAttribute(3, "Title", "Step 2");
            builder.AddAttribute(4, "Disabled", true);
            builder.CloseComponent();
        };

        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ChildContent, steps));

        cut.Find(".arcadia-step--disabled").Should().NotBeNull();
    }

    [Fact]
    public void StepTitle_Renders()
    {
        var cut = Render<ArcadiaStepper>(p => p
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ChildContent, ThreeSteps));

        var titles = cut.FindAll(".arcadia-step__title");
        titles.Should().HaveCount(3);
        titles[0].TextContent.Should().Be("Step 1");
    }
}
