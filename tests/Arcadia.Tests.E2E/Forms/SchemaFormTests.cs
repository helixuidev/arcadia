using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Forms;

[TestFixture]
public class SchemaFormTests : PageTest
{
    /// <summary>
    /// Navigate to the "Schema Form" tab inside the Forms section.
    /// </summary>
    private async Task NavigateToSchemaFormTab()
    {
        await Page.GotoAsync(TestConstants.BaseUrl + "/",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);

        // Expand Forms section if collapsed
        var schemaBtn = Page.Locator("button.gallery__nav-btn:has-text('Schema Form')");
        var count = await schemaBtn.CountAsync();

        if (count == 0)
        {
            var formsHeader = Page.Locator("button.gallery__nav-group:has-text('Forms')");
            await formsHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        schemaBtn = Page.Locator("button.gallery__nav-btn:has-text('Schema Form')");
        await schemaBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);
    }

    [Test]
    public async Task ContactForm_RendersWithExpectedFields()
    {
        await NavigateToSchemaFormTab();

        // The schema defines: Full Name, Email, Contact Method, Message
        var nameField = Page.Locator("label:has-text('Full Name')");
        await Expect(nameField.First).ToBeVisibleAsync();

        var emailField = Page.Locator("label:has-text('Email')");
        await Expect(emailField.First).ToBeVisibleAsync();

        var methodField = Page.Locator("label:has-text('Contact Method')");
        await Expect(methodField.First).ToBeVisibleAsync();

        var messageField = Page.Locator("label:has-text('Message')");
        await Expect(messageField.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ContactForm_RequiredFieldsHaveIndicators()
    {
        await NavigateToSchemaFormTab();

        // Required fields (Name, Email, Contact Method, Message, terms) should have
        // an asterisk or "required" indicator near their labels
        var requiredIndicators = Page.Locator(".arcadia-field__required, .arcadia-form__required, span:has-text('*')");
        var count = await requiredIndicators.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1),
            "Required fields should have visual required indicators");
    }

    [Test]
    public async Task ContactForm_SubmitButtonExists()
    {
        await NavigateToSchemaFormTab();

        // Schema defines SubmitText = "Send Message"
        var submitButton = Page.Locator("button:has-text('Send Message')");
        await Expect(submitButton.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ContactForm_PhoneFieldAppearsWhenPhoneMethodSelected()
    {
        await NavigateToSchemaFormTab();

        // Initially, the Phone field should NOT be visible (it's conditional)
        var phoneField = Page.Locator("label:has-text('Phone')");
        var initialCount = await phoneField.CountAsync();

        // Select "Phone" in the Contact Method dropdown
        var methodSelect = Page.Locator("select").First;
        await methodSelect.SelectOptionAsync("Phone");
        await Page.WaitForTimeoutAsync(500);

        // Now the Phone field should appear
        phoneField = Page.Locator("label:has-text('Phone')");
        var afterCount = await phoneField.CountAsync();
        Assert.That(afterCount, Is.GreaterThan(initialCount),
            "Phone field should appear after selecting 'Phone' as contact method");
    }
}
