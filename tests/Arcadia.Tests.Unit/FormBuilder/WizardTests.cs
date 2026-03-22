using Bunit;
using FluentAssertions;
using Arcadia.FormBuilder.Components;
using Arcadia.FormBuilder.Components.Fields;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class WizardTests : BunitContext
{
    [Fact]
    public void Renders_StepNavigation()
    {
        var cut = Render<HelixWizard>(p => p
            .Add(c => c.CurrentStep, 0)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<HelixWizardStep>(0);
                builder.AddAttribute(1, "Title", "Step 1");
                builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Content 1")));
                builder.CloseComponent();
                builder.OpenComponent<HelixWizardStep>(3);
                builder.AddAttribute(4, "Title", "Step 2");
                builder.AddAttribute(5, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Content 2")));
                builder.CloseComponent();
            }));

        var buttons = cut.FindAll(".arcadia-wizard__step-btn");
        buttons.Should().HaveCount(2);
        buttons[0].TextContent.Should().Contain("Step 1");
        buttons[1].TextContent.Should().Contain("Step 2");
    }

    [Fact]
    public void Shows_CurrentStep_Content()
    {
        var cut = Render<HelixWizard>(p => p
            .Add(c => c.CurrentStep, 0)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<HelixWizardStep>(0);
                builder.AddAttribute(1, "Title", "Step 1");
                builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "First Step Content")));
                builder.CloseComponent();
                builder.OpenComponent<HelixWizardStep>(3);
                builder.AddAttribute(4, "Title", "Step 2");
                builder.AddAttribute(5, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Second Step Content")));
                builder.CloseComponent();
            }));

        cut.Find(".arcadia-wizard__panel").TextContent.Should().Contain("First Step Content");
        cut.Find(".arcadia-wizard__panel").TextContent.Should().NotContain("Second Step Content");
    }

    [Fact]
    public void Shows_ProgressBar()
    {
        var cut = Render<HelixWizard>(p => p
            .Add(c => c.CurrentStep, 0)
            .Add(c => c.ShowProgress, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<HelixWizardStep>(0);
                builder.AddAttribute(1, "Title", "Step 1");
                builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => { }));
                builder.CloseComponent();
                builder.OpenComponent<HelixWizardStep>(3);
                builder.AddAttribute(4, "Title", "Step 2");
                builder.AddAttribute(5, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => { }));
                builder.CloseComponent();
            }));

        cut.FindAll(".arcadia-wizard__progress").Should().HaveCount(1);
    }

    [Fact]
    public void Shows_NextButton_NotComplete_OnNonLastStep()
    {
        var cut = Render<HelixWizard>(p => p
            .Add(c => c.CurrentStep, 0)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<HelixWizardStep>(0);
                builder.AddAttribute(1, "Title", "S1");
                builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => { }));
                builder.CloseComponent();
                builder.OpenComponent<HelixWizardStep>(3);
                builder.AddAttribute(4, "Title", "S2");
                builder.AddAttribute(5, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => { }));
                builder.CloseComponent();
            }));

        cut.FindAll(".arcadia-wizard__btn--next").Should().HaveCount(1);
        cut.FindAll(".arcadia-wizard__btn--complete").Should().BeEmpty();
        cut.FindAll(".arcadia-wizard__btn--prev").Should().BeEmpty();
    }
}
