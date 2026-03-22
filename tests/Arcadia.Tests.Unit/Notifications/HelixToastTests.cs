using Bunit;
using FluentAssertions;
using Arcadia.Notifications;
using Xunit;

namespace Arcadia.Tests.Unit.Notifications;

public class HelixToastTests : BunitContext
{
    [Theory]
    [InlineData(ToastLevel.Info, "arcadia-toast--info")]
    [InlineData(ToastLevel.Success, "arcadia-toast--success")]
    [InlineData(ToastLevel.Warning, "arcadia-toast--warning")]
    [InlineData(ToastLevel.Error, "arcadia-toast--error")]
    public void Renders_WithLevelClass(ToastLevel level, string expectedClass)
    {
        var toast = new ToastModel { Level = level, Message = "Test" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.Find(".arcadia-toast").ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void Renders_Message()
    {
        var toast = new ToastModel { Message = "Something happened" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.Find(".arcadia-toast__message").TextContent.Should().Be("Something happened");
    }

    [Fact]
    public void Renders_Title_WhenProvided()
    {
        var toast = new ToastModel { Title = "Alert", Message = "Details here" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.Find(".arcadia-toast__title").TextContent.Should().Be("Alert");
    }

    [Fact]
    public void DoesNotRender_Title_WhenEmpty()
    {
        var toast = new ToastModel { Message = "No title" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.FindAll(".arcadia-toast__title").Should().BeEmpty();
    }

    [Fact]
    public void Renders_DismissButton_WhenDismissible()
    {
        var toast = new ToastModel { Message = "Test", Dismissible = true };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.FindAll(".arcadia-toast__dismiss").Should().HaveCount(1);
    }

    [Fact]
    public void DoesNotRender_DismissButton_WhenNotDismissible()
    {
        var toast = new ToastModel { Message = "Test", Dismissible = false };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.FindAll(".arcadia-toast__dismiss").Should().BeEmpty();
    }

    [Fact]
    public void DismissButton_InvokesCallback()
    {
        var toast = new ToastModel { Message = "Test" };
        var dismissed = false;

        var cut = Render<HelixToast>(p =>
            p.Add(c => c.Toast, toast)
             .Add(c => c.OnDismiss, () => dismissed = true));

        cut.Find(".arcadia-toast__dismiss").Click();

        dismissed.Should().BeTrue();
    }

    [Fact]
    public void ErrorToast_HasAlertRole()
    {
        var toast = new ToastModel { Level = ToastLevel.Error, Message = "Error" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.Find(".arcadia-toast").GetAttribute("role").Should().Be("alert");
        cut.Find(".arcadia-toast").GetAttribute("aria-live").Should().Be("assertive");
    }

    [Fact]
    public void InfoToast_HasStatusRole()
    {
        var toast = new ToastModel { Level = ToastLevel.Info, Message = "Info" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.Find(".arcadia-toast").GetAttribute("role").Should().Be("status");
        cut.Find(".arcadia-toast").GetAttribute("aria-live").Should().Be("polite");
    }

    [Fact]
    public void Renders_Icon()
    {
        var toast = new ToastModel { Level = ToastLevel.Success, Message = "Done" };

        var cut = Render<HelixToast>(p => p.Add(c => c.Toast, toast));

        cut.FindAll(".arcadia-toast__icon svg").Should().HaveCount(1);
    }
}
