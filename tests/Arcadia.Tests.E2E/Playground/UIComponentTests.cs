using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Playground;

[TestFixture]
public class UIComponentTests : PageTest
{
    /// <summary>
    /// Navigate to home page and click the "Dialog / Tabs / Tooltip" button in the UI section.
    /// On the main gallery page (/), the UI section is a tab switcher (buttons), not nav links.
    /// </summary>
    private async Task NavigateToUITab()
    {
        await Page.GotoAsync(TestConstants.BaseUrl + "/",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);

        // Click the "Dialog / Tabs / Tooltip" button in the UI section
        var uiButton = Page.Locator("button:has-text('Dialog / Tabs / Tooltip')");
        var uiCount = await uiButton.CountAsync();

        if (uiCount > 0)
        {
            await uiButton.First.ClickAsync();
        }
        else
        {
            // UI section might be collapsed; expand it first
            var uiHeader = Page.Locator(".gallery__nav-group >> text='UI'");
            await uiHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
            uiButton = Page.Locator("button:has-text('Dialog / Tabs / Tooltip')");
            await uiButton.First.ClickAsync();
        }

        await Page.WaitForTimeoutAsync(1000);
    }

    // ── Dialog ──

    [Test]
    public async Task Dialog_OpenButtonExists()
    {
        await NavigateToUITab();

        var openButton = Page.Locator("button:has-text('Open Dialog')");
        await Expect(openButton.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dialog_ClickingOpenShowsOverlay()
    {
        await NavigateToUITab();

        var openButton = Page.Locator("button:has-text('Open Dialog')");
        await openButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Dialog overlay/backdrop should appear
        var overlay = Page.Locator(".arcadia-dialog__overlay, .arcadia-dialog__backdrop, .arcadia-dialog");
        await Expect(overlay.First).ToBeVisibleAsync();

        // Dialog title should be visible
        var dialogTitle = Page.Locator("text=Confirm Action");
        await Expect(dialogTitle.First).ToBeVisibleAsync();

        // Dialog should have Confirm and Cancel buttons
        var confirmBtn = Page.Locator("button:has-text('Confirm')");
        await Expect(confirmBtn.First).ToBeVisibleAsync();

        var cancelBtn = Page.Locator("button:has-text('Cancel')");
        await Expect(cancelBtn.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dialog_ClickCancelClosesDialog()
    {
        await NavigateToUITab();

        var openButton = Page.Locator("button:has-text('Open Dialog')");
        await openButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var cancelBtn = Page.Locator("button:has-text('Cancel')");
        await cancelBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Dialog should no longer be visible
        var dialog = Page.Locator(".arcadia-dialog");
        var count = await dialog.CountAsync();
        if (count > 0)
        {
            await Expect(dialog.First).Not.ToBeVisibleAsync();
        }
    }

    // ── Tabs ──

    [Test]
    public async Task Tabs_ClickingTabChangesActivePanel()
    {
        await NavigateToUITab();

        // Find the Tabs section
        var tabsTitle = Page.Locator(".gallery__showcase-title:has-text('Tabs')");
        await Expect(tabsTitle.First).ToBeVisibleAsync();

        // Find tab buttons
        var overviewTab = Page.Locator("[role='tab']:has-text('Overview'), button:has-text('Overview')");
        var featuresTab = Page.Locator("[role='tab']:has-text('Features'), button:has-text('Features')");

        await Expect(overviewTab.First).ToBeVisibleAsync();
        await Expect(featuresTab.First).ToBeVisibleAsync();

        // Click "Features" tab
        await featuresTab.First.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // The Features panel content should now be visible
        var featuresContent = Page.Locator("text=Feature details go here");
        await Expect(featuresContent.First).ToBeVisibleAsync();

        // Click back to "Overview"
        await overviewTab.First.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var overviewContent = Page.Locator("text=This is the overview panel");
        await Expect(overviewContent.First).ToBeVisibleAsync();
    }

    // ── Tooltip ──

    [Test]
    public async Task Tooltip_HoveringButtonShowsTooltipText()
    {
        await NavigateToUITab();

        // Find the tooltip section buttons — "Top", "Right", "Bottom", "Left"
        var tooltipTitle = Page.Locator(".gallery__showcase-title:has-text('Tooltip')");
        await Expect(tooltipTitle.First).ToBeVisibleAsync();

        // Hover over the "Top" button
        var topButton = Page.Locator("button:has-text('Top')").First;
        await topButton.HoverAsync();
        await Page.WaitForTimeoutAsync(500);

        // Tooltip text should appear
        var tooltipText = Page.Locator("text=Tooltip on top");
        var count = await tooltipText.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1),
            "Hovering the 'Top' button should show tooltip with 'Tooltip on top' text");
    }

    // ── Card ──

    [Test]
    public async Task Card_TwoCardsRenderWithTitles()
    {
        await NavigateToUITab();

        var cardTitle = Page.Locator(".gallery__showcase-title:has-text('Card')");
        await Expect(cardTitle.First).ToBeVisibleAsync();

        // Check for the two card titles
        var revenueCard = Page.Locator("text=Revenue Report");
        await Expect(revenueCard.First).ToBeVisibleAsync();

        var clickableCard = Page.Locator("text=Clickable Card");
        await Expect(clickableCard.First).ToBeVisibleAsync();
    }

    // ── Badge ──

    [Test]
    public async Task Badge_ElementsRenderWithContent()
    {
        await NavigateToUITab();

        var badgeTitle = Page.Locator(".gallery__showcase-title:has-text('Badge')");
        await Expect(badgeTitle.First).ToBeVisibleAsync();

        // Badge with content "3"
        var badge3 = Page.Locator(".arcadia-badge");
        var badgeCount = await badge3.CountAsync();
        Assert.That(badgeCount, Is.GreaterThanOrEqualTo(1),
            "Should have at least 1 badge element rendered");

        // Check that buttons with badges are visible
        var inboxBtn = Page.Locator("button:has-text('Inbox')");
        await Expect(inboxBtn.First).ToBeVisibleAsync();

        var alertsBtn = Page.Locator("button:has-text('Alerts')");
        await Expect(alertsBtn.First).ToBeVisibleAsync();
    }

    // ── Accordion ──

    [Test]
    public async Task Accordion_ClickingItemExpandsIt()
    {
        await NavigateToUITab();

        var accordionTitle = Page.Locator(".gallery__showcase-title:has-text('Accordion')");
        await Expect(accordionTitle.First).ToBeVisibleAsync();

        // Click the first accordion item header "What is Arcadia Controls?"
        var firstItem = Page.Locator("text=What is Arcadia Controls?");
        await firstItem.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // The expanded content should be visible
        var expandedContent = Page.Locator("text=commercial Blazor component library");
        await Expect(expandedContent.First).ToBeVisibleAsync();

        // Click the second item
        var secondItem = Page.Locator("text=How do I install it?");
        await secondItem.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        var secondContent = Page.Locator("text=dotnet add package");
        await Expect(secondContent.First).ToBeVisibleAsync();
    }

    // ── Breadcrumb ──

    [Test]
    public async Task Breadcrumb_RendersWithCorrectItems()
    {
        await NavigateToUITab();

        var breadcrumbTitle = Page.Locator(".gallery__showcase-title:has-text('Breadcrumb')");
        await Expect(breadcrumbTitle.First).ToBeVisibleAsync();

        // Check breadcrumb items: Home > Documentation > Charts > Line Chart
        var homeItem = Page.Locator(".arcadia-breadcrumb a:has-text('Home'), .arcadia-breadcrumb >> text='Home'");
        await Expect(homeItem.First).ToBeVisibleAsync();

        var docsItem = Page.Locator("text=Documentation");
        await Expect(docsItem.First).ToBeVisibleAsync();

        var chartsItem = Page.Locator(".arcadia-breadcrumb >> text='Charts'");
        await Expect(chartsItem.First).ToBeVisibleAsync();

        var lineChartItem = Page.Locator("text=Line Chart");
        await Expect(lineChartItem.First).ToBeVisibleAsync();
    }
}
