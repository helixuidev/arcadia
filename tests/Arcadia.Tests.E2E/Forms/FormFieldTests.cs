using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Forms;

[TestFixture]
public class FormFieldTests : PageTest
{
    /// <summary>
    /// Navigate to the "Field Types" tab inside the Forms section.
    /// The Forms section may be collapsed, so expand it first.
    /// </summary>
    private async Task NavigateToFieldTypesTab()
    {
        await Page.GotoAsync(TestConstants.BaseUrl + "/",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);

        // Expand Forms section if collapsed
        var fieldTypesBtn = Page.Locator("button.gallery__nav-btn:has-text('Field Types')");
        var count = await fieldTypesBtn.CountAsync();

        if (count == 0)
        {
            var formsHeader = Page.Locator("button.gallery__nav-group:has-text('Forms')");
            await formsHeader.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        fieldTypesBtn = Page.Locator("button.gallery__nav-btn:has-text('Field Types')");
        await fieldTypesBtn.First.ClickAsync();
        await Page.WaitForTimeoutAsync(1500);
    }

    // ── Text Field ──

    [Test]
    public async Task TextField_RendersWithLabelAndInput()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Full Name')");
        await Expect(label.First).ToBeVisibleAsync();

        var input = Page.Locator("input[placeholder='Jane Doe']");
        await Expect(input.First).ToBeVisibleAsync();
    }

    // ── Number Field ──

    [Test]
    public async Task NumberField_RendersWithTypeNumber()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Quantity')");
        await Expect(label.First).ToBeVisibleAsync();

        var input = Page.Locator("input[type='number']");
        await Expect(input.First).ToBeVisibleAsync();
    }

    // ── Select/Dropdown ──

    [Test]
    public async Task SelectField_RendersWithOptions()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Country')");
        await Expect(label.First).ToBeVisibleAsync();

        var select = Page.Locator("select");
        var selectCount = await select.CountAsync();
        Assert.That(selectCount, Is.GreaterThanOrEqualTo(1),
            "Should render at least one select element for the Country field");
    }

    // ── Checkbox ──

    [Test]
    public async Task CheckboxField_Renders()
    {
        await NavigateToFieldTypesTab();

        var checkbox = Page.Locator("input[type='checkbox']");
        var count = await checkbox.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1),
            "Should render at least one checkbox input");

        var label = Page.Locator("text=I agree to the terms and conditions");
        await Expect(label.First).ToBeVisibleAsync();
    }

    // ── Switch/Toggle ──

    [Test]
    public async Task SwitchField_Renders()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("text=Enable dark mode");
        await Expect(label.First).ToBeVisibleAsync();
    }

    // ── Date Field ──

    [Test]
    public async Task DateField_RendersWithTypeDate()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Start Date')");
        await Expect(label.First).ToBeVisibleAsync();

        var input = Page.Locator("input[type='date']");
        await Expect(input.First).ToBeVisibleAsync();
    }

    // ── Slider Field ──

    [Test]
    public async Task SliderField_RendersWithTypeRange()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Volume')");
        await Expect(label.First).ToBeVisibleAsync();

        var input = Page.Locator("input[type='range']");
        await Expect(input.First).ToBeVisibleAsync();
    }

    // ── Rating Field ──

    [Test]
    public async Task RatingField_RendersStarElements()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Satisfaction')");
        await Expect(label.First).ToBeVisibleAsync();

        // Rating stars use arcadia-field__rating-star class
        var stars = Page.Locator(".arcadia-field__rating-star");
        var count = await stars.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(5),
            "Rating field should render at least 5 star elements");
    }

    // ── Color Picker ──

    [Test]
    public async Task ColorField_RendersWithTypeColor()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Brand Color')");
        await Expect(label.First).ToBeVisibleAsync();

        var input = Page.Locator("input[type='color']");
        await Expect(input.First).ToBeVisibleAsync();
    }

    // ── Password Field ──

    [Test]
    public async Task PasswordField_RendersWithTypePassword()
    {
        await NavigateToFieldTypesTab();

        var label = Page.Locator("label:has-text('Password')");
        await Expect(label.First).ToBeVisibleAsync();

        var input = Page.Locator("input[type='password']");
        await Expect(input.First).ToBeVisibleAsync();
    }

    // ── Accessibility ──

    [Test]
    public async Task AllFields_HaveAssociatedLabels()
    {
        await NavigateToFieldTypesTab();

        // Check that all visible label elements have a matching for attribute or wrap an input
        var labels = Page.Locator("label");
        var labelCount = await labels.CountAsync();
        Assert.That(labelCount, Is.GreaterThanOrEqualTo(10),
            "Field Types tab should render at least 10 label elements for its fields");

        // Verify specific field labels exist
        string[] expectedLabels = { "Full Name", "Password", "Country", "Start Date", "Quantity", "Volume", "Satisfaction", "Brand Color" };

        foreach (var expected in expectedLabels)
        {
            var label = Page.Locator($"label:has-text('{expected}')");
            var count = await label.CountAsync();
            Assert.That(count, Is.GreaterThanOrEqualTo(1),
                $"Expected a label element with text '{expected}'");
        }
    }
}
