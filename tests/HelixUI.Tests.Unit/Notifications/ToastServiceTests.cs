using FluentAssertions;
using HelixUI.Notifications;
using Xunit;

namespace HelixUI.Tests.Unit.Notifications;

public class ToastServiceTests
{
    [Fact]
    public void Show_AddsToast()
    {
        var service = new ToastService();

        service.Show("Hello");

        service.Toasts.Should().HaveCount(1);
        service.Toasts[0].Message.Should().Be("Hello");
    }

    [Fact]
    public void Show_ReturnsId()
    {
        var service = new ToastService();

        var id = service.Show("Hello");

        id.Should().NotBeNullOrEmpty();
        service.Toasts[0].Id.Should().Be(id);
    }

    [Fact]
    public void Show_FiresOnToastsChanged()
    {
        var service = new ToastService();
        var fired = false;
        service.OnToastsChanged += () => fired = true;

        service.Show("Hello");

        fired.Should().BeTrue();
    }

    [Fact]
    public void ShowInfo_SetsInfoLevel()
    {
        var service = new ToastService();

        service.ShowInfo("Test");

        service.Toasts[0].Level.Should().Be(ToastLevel.Info);
    }

    [Fact]
    public void ShowSuccess_SetsSuccessLevel()
    {
        var service = new ToastService();

        service.ShowSuccess("Test");

        service.Toasts[0].Level.Should().Be(ToastLevel.Success);
    }

    [Fact]
    public void ShowWarning_SetsWarningLevel()
    {
        var service = new ToastService();

        service.ShowWarning("Test");

        service.Toasts[0].Level.Should().Be(ToastLevel.Warning);
    }

    [Fact]
    public void ShowError_SetsErrorLevel()
    {
        var service = new ToastService();

        service.ShowError("Test");

        service.Toasts[0].Level.Should().Be(ToastLevel.Error);
    }

    [Fact]
    public void ShowError_DefaultsToNoDismiss()
    {
        var service = new ToastService();

        service.ShowError("Test");

        service.Toasts[0].Duration.Should().Be(0);
    }

    [Fact]
    public void Show_WithTitle_SetsTitle()
    {
        var service = new ToastService();

        service.ShowSuccess("Body text", "Title");

        service.Toasts[0].Title.Should().Be("Title");
        service.Toasts[0].Message.Should().Be("Body text");
    }

    [Fact]
    public void Dismiss_RemovesToast()
    {
        var service = new ToastService();
        var id = service.Show("Hello");

        service.Dismiss(id);

        service.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void Dismiss_FiresOnToastsChanged()
    {
        var service = new ToastService();
        var id = service.Show("Hello");
        var fireCount = 0;
        service.OnToastsChanged += () => fireCount++;

        service.Dismiss(id);

        fireCount.Should().Be(1);
    }

    [Fact]
    public void Dismiss_UnknownId_DoesNotFire()
    {
        var service = new ToastService();
        service.Show("Hello");
        var fireCount = 0;
        service.OnToastsChanged += () => fireCount++;

        service.Dismiss("nonexistent");

        fireCount.Should().Be(0);
    }

    [Fact]
    public void DismissAll_ClearsAllToasts()
    {
        var service = new ToastService();
        service.Show("One");
        service.Show("Two");
        service.Show("Three");

        service.DismissAll();

        service.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void DismissAll_WhenEmpty_DoesNotFire()
    {
        var service = new ToastService();
        var fired = false;
        service.OnToastsChanged += () => fired = true;

        service.DismissAll();

        fired.Should().BeFalse();
    }

    [Fact]
    public void Show_MultipleToasts_MaintainsOrder()
    {
        var service = new ToastService();

        service.Show("First");
        service.Show("Second");
        service.Show("Third");

        service.Toasts.Should().HaveCount(3);
        service.Toasts[0].Message.Should().Be("First");
        service.Toasts[2].Message.Should().Be("Third");
    }

    [Fact]
    public void Show_CustomDuration()
    {
        var service = new ToastService();

        service.Show("Test", duration: 10000);

        service.Toasts[0].Duration.Should().Be(10000);
    }
}
