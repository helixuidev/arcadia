using Bunit;
using FluentAssertions;
using HelixUI.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelixUI.Tests.Unit.Notifications;

public class HelixToastContainerTests : BunitContext
{
    public HelixToastContainerTests()
    {
        Services.AddScoped<ToastService>();
    }

    [Fact]
    public void Renders_EmptyContainer()
    {
        var cut = Render<HelixToastContainer>();

        cut.Find(".helix-toast-container").Should().NotBeNull();
        cut.FindAll(".helix-toast").Should().BeEmpty();
    }

    [Fact]
    public void Renders_DefaultPosition_TopRight()
    {
        var cut = Render<HelixToastContainer>();

        cut.Find(".helix-toast-container").ClassList.Should().Contain("helix-toast-container--top-right");
    }

    [Theory]
    [InlineData(ToastPosition.TopLeft, "helix-toast-container--top-left")]
    [InlineData(ToastPosition.BottomRight, "helix-toast-container--bottom-right")]
    [InlineData(ToastPosition.BottomCenter, "helix-toast-container--bottom-center")]
    public void Renders_WithPositionClass(ToastPosition position, string expectedClass)
    {
        var cut = Render<HelixToastContainer>(p => p.Add(c => c.Position, position));

        cut.Find(".helix-toast-container").ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void Shows_Toast_WhenServiceAdds()
    {
        var toastService = Services.GetRequiredService<ToastService>();
        var cut = Render<HelixToastContainer>();

        toastService.ShowInfo("Hello world");

        cut.FindAll(".helix-toast").Should().HaveCount(1);
    }

    [Fact]
    public void Shows_MultipleToasts()
    {
        var toastService = Services.GetRequiredService<ToastService>();
        var cut = Render<HelixToastContainer>();

        toastService.ShowInfo("First");
        toastService.ShowSuccess("Second");
        toastService.ShowWarning("Third");

        cut.FindAll(".helix-toast").Should().HaveCount(3);
    }

    [Fact]
    public void Dismiss_RemovesToast()
    {
        var toastService = Services.GetRequiredService<ToastService>();
        var cut = Render<HelixToastContainer>();

        var id = toastService.ShowInfo("Will be dismissed");
        cut.FindAll(".helix-toast").Should().HaveCount(1);

        toastService.Dismiss(id);
        cut.FindAll(".helix-toast").Should().BeEmpty();
    }

    [Fact]
    public void Renders_WithCustomClass()
    {
        var cut = Render<HelixToastContainer>(p => p.Add(c => c.Class, "my-toasts"));

        cut.Find(".helix-toast-container").ClassList.Should().Contain("my-toasts");
    }
}
