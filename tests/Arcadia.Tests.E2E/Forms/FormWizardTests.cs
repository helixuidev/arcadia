using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Forms;

[TestFixture]
public class FormWizardTests : PageTest
{
    /// <summary>
    /// Navigate to the "Wizard" tab inside the Forms section.
    /// </summary>
    private async Task NavigateToWizardTab()
    {
        await Page.GotoAsync(TestConstants.BaseUrl + "/",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);

        // Expand Forms section if collapsed
        var wizardBtn = Page.Locator("button.gallery__nav-btn:has-text('Wizard')");
        var count = await wizardBtn.CountAsync();

        if (count == 0)
        {
            var formsHeader = Page.Locator("button.gallery__nav-group:has-text('Forms')");
            await formsHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        wizardBtn = Page.Locator("button.gallery__nav-btn:has-text('Wizard')");
        await wizardBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);
    }

    [Test]
    public async Task Wizard_RendersWithStepIndicators()
    {
        await NavigateToWizardTab();

        // Wizard should show step titles: Personal, Preferences, Review
        var personalStep = Page.Locator("text=Personal");
        await Expect(personalStep.First).ToBeVisibleAsync();

        var preferencesStep = Page.Locator("text=Preferences");
        await Expect(preferencesStep.First).ToBeVisibleAsync();

        var reviewStep = Page.Locator("text=Review");
        await Expect(reviewStep.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Wizard_FirstStepContentIsVisible()
    {
        await NavigateToWizardTab();

        // First step should show "Full Name" and "Email" fields
        var nameField = Page.Locator("label:has-text('Full Name')");
        await Expect(nameField.First).ToBeVisibleAsync();

        var emailField = Page.Locator("label:has-text('Email')");
        await Expect(emailField.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Wizard_NextButtonAdvancesToStep2()
    {
        await NavigateToWizardTab();

        // Click Next to go to step 2 (Preferences)
        var nextButton = Page.Locator("button:has-text('Next')");
        await Expect(nextButton.First).ToBeVisibleAsync();
        await nextButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Step 2 should show the Rating and Slider fields
        var ratingLabel = Page.Locator("text=How would you rate our product?");
        await Expect(ratingLabel.First).ToBeVisibleAsync();

        var frequencyLabel = Page.Locator("label:has-text('Email frequency')");
        await Expect(frequencyLabel.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Wizard_PreviousButtonGoesBack()
    {
        await NavigateToWizardTab();

        // Go to step 2
        var nextButton = Page.Locator("button:has-text('Next')");
        await nextButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Go back to step 1
        var prevButton = Page.Locator("button:has-text('Previous')");
        await Expect(prevButton.First).ToBeVisibleAsync();
        await prevButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Step 1 fields should be visible again
        var nameField = Page.Locator("label:has-text('Full Name')");
        await Expect(nameField.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Wizard_FinalStepShowsCompletionAction()
    {
        await NavigateToWizardTab();

        // Navigate to step 2
        var nextButton = Page.Locator("button:has-text('Next')");
        await nextButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Navigate to step 3 (Review)
        nextButton = Page.Locator("button:has-text('Next')");
        await nextButton.First.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Review step should show summary data labels
        var nameLabel = Page.Locator("text=Name:");
        await Expect(nameLabel.First).ToBeVisibleAsync();

        var emailLabel = Page.Locator("text=Email:");
        await Expect(emailLabel.First).ToBeVisibleAsync();

        // Final step should have a Submit/Complete button
        var completeButton = Page.Locator("button:has-text('Complete'), button:has-text('Submit'), button:has-text('Finish')");
        await Expect(completeButton.First).ToBeVisibleAsync();
    }
}
