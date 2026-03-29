using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Notifications;

[TestFixture]
public class ToastTests : PageTest
{
    /// <summary>
    /// Navigate to the "Notifications" tab inside the UI section.
    /// </summary>
    private async Task NavigateToNotificationsTab()
    {
        await Page.GotoAsync(TestConstants.BaseUrl + "/",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);

        // Expand UI section if collapsed
        var notificationsBtn = Page.Locator("button.gallery__nav-btn:has-text('Notifications')");
        var count = await notificationsBtn.CountAsync();

        if (count == 0)
        {
            var uiHeader = Page.Locator("button.gallery__nav-group:has-text('UI')");
            await uiHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        notificationsBtn = Page.Locator("button.gallery__nav-btn:has-text('Notifications')");
        await notificationsBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);
    }

    [Test]
    public async Task Toast_FourSeverityButtonsVisible()
    {
        await NavigateToNotificationsTab();

        var infoBtn = Page.Locator("button.gallery__toast-btn--info:has-text('Info')");
        await Expect(infoBtn.First).ToBeVisibleAsync();

        var successBtn = Page.Locator("button.gallery__toast-btn--success:has-text('Success')");
        await Expect(successBtn.First).ToBeVisibleAsync();

        var warningBtn = Page.Locator("button.gallery__toast-btn--warning:has-text('Warning')");
        await Expect(warningBtn.First).ToBeVisibleAsync();

        var errorBtn = Page.Locator("button.gallery__toast-btn--error:has-text('Error')");
        await Expect(errorBtn.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Toast_ClickingInfoShowsToastNotification()
    {
        await NavigateToNotificationsTab();

        var infoBtn = Page.Locator("button.gallery__toast-btn--info:has-text('Info')");
        await infoBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // A toast notification should appear
        var toast = Page.Locator(".arcadia-toast, .arcadia-toast-container .arcadia-toast__item, [role='alert']");
        var count = await toast.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1),
            "Clicking Info should show at least one toast notification");
    }

    [Test]
    public async Task Toast_HasCorrectContentText()
    {
        await NavigateToNotificationsTab();

        var infoBtn = Page.Locator("button.gallery__toast-btn--info:has-text('Info')");
        await infoBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // The toast should contain the info message text
        var toastText = Page.Locator("text=This is an informational message.");
        var count = await toastText.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1),
            "Toast should display the informational message text");
    }

    [Test]
    public async Task Toast_DismissAllButtonExists()
    {
        await NavigateToNotificationsTab();

        var dismissBtn = Page.Locator("button:has-text('Dismiss All')");
        await Expect(dismissBtn.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Toast_AutoDismissesAfterTimeout()
    {
        await NavigateToNotificationsTab();

        var infoBtn = Page.Locator("button.gallery__toast-btn--info:has-text('Info')");
        await infoBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Verify toast appeared
        var toast = Page.Locator(".arcadia-toast").First;
        await Expect(toast).ToBeVisibleAsync();

        // Wait for auto-dismiss using Playwright's built-in wait (no fixed timeout)
        await Expect(toast).ToBeHiddenAsync(new() { Timeout = 15000 });
    }
}
