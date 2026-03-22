using Bunit;
using FluentAssertions;
using Arcadia.FormBuilder.Components.Layout;
using Xunit;

namespace Arcadia.Tests.Unit.FormBuilder;

public class LayoutTests : BunitContext
{
    [Fact]
    public void FormDivider_Renders_Separator()
    {
        var cut = Render<FormDivider>();

        cut.Find("[role='separator']").Should().NotBeNull();
    }

    [Fact]
    public void FormDivider_WithLabel_ShowsLabel()
    {
        var cut = Render<FormDivider>(p => p.Add(c => c.Label, "Section Break"));

        cut.Find(".arcadia-form-divider__label").TextContent.Should().Be("Section Break");
        cut.Find(".arcadia-form-divider").ClassList.Should().Contain("arcadia-form-divider--labeled");
    }

    [Fact]
    public void FormAlert_Renders_Info()
    {
        var cut = Render<FormAlert>(p =>
            p.Add(c => c.Level, "info")
             .Add(c => c.Title, "Note")
             .AddChildContent("Some info text"));

        cut.Find(".arcadia-form-alert--info").Should().NotBeNull();
        cut.Find(".arcadia-form-alert__title").TextContent.Should().Be("Note");
        cut.Find(".arcadia-form-alert__message").TextContent.Should().Contain("Some info text");
    }

    [Fact]
    public void FormAlert_Error_HasAlertRole()
    {
        var cut = Render<FormAlert>(p =>
            p.Add(c => c.Level, "error")
             .AddChildContent("Error!"));

        cut.Find("[role='alert']").Should().NotBeNull();
    }

    [Fact]
    public void FormAlert_Info_HasStatusRole()
    {
        var cut = Render<FormAlert>(p =>
            p.Add(c => c.Level, "info")
             .AddChildContent("Info"));

        cut.Find("[role='status']").Should().NotBeNull();
    }

    [Fact]
    public void FormSection_Collapsible_Toggles()
    {
        var cut = Render<FormSection>(p =>
            p.Add(c => c.Title, "Details")
             .Add(c => c.Collapsible, true)
             .Add(c => c.Collapsed, false)
             .AddChildContent("<p>Content</p>"));

        cut.FindAll(".arcadia-form-section__content").Should().HaveCount(1);

        cut.Find(".arcadia-form-section__toggle").Click();

        cut.FindAll(".arcadia-form-section__content").Should().BeEmpty();
    }

    [Fact]
    public void FormActions_ShowsSubmitAndCancel()
    {
        var cut = Render<FormActions>(p =>
            p.Add(c => c.SubmitText, "Save")
             .Add(c => c.CancelText, "Discard")
             .Add(c => c.ShowCancel, true));

        cut.Find(".arcadia-form-actions__submit").TextContent.Should().Contain("Save");
        cut.Find(".arcadia-form-actions__cancel").TextContent.Should().Contain("Discard");
    }

    [Fact]
    public void FormActions_Submitting_ShowsSpinner()
    {
        var cut = Render<FormActions>(p =>
            p.Add(c => c.IsSubmitting, true)
             .Add(c => c.SubmittingText, "Saving..."));

        cut.Find(".arcadia-form-actions__submit").TextContent.Should().Contain("Saving...");
        cut.FindAll(".arcadia-form-actions__spinner").Should().HaveCount(1);
    }
}
