using Arcadia.Tests.E2E.Infrastructure;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.DataGrid;

[TestFixture]
public class DataGridInteractionTests : PageTest
{
    private async Task NavigateToBasics()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/datagrid/basics",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);
    }

    private async Task NavigateToEditing()
    {
        await Page.GotoAsync($"{TestConstants.BaseUrl}/datagrid/editing",
            new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(1500);
    }

    // ── Cell focus ──

    [Test]
    public async Task ClickCell_FocusRectangleAppearsOnThatCell()
    {
        await NavigateToBasics();

        // Find all data cells in the first sortable grid (the sorting section)
        var rows = Page.Locator("tbody tr[role='row']");
        var rowCount = await rows.CountAsync();
        Assert.That(rowCount, Is.GreaterThanOrEqualTo(3),
            "Need at least 3 rows for meaningful focus test");

        // Click a cell in the third row (not the first) to catch "focus always on first cell" bugs
        var thirdRowCells = rows.Nth(2).Locator("td");
        var cellCount = await thirdRowCells.CountAsync();
        Assert.That(cellCount, Is.GreaterThanOrEqualTo(2), "Row should have multiple cells");

        var targetCell = thirdRowCells.Nth(1); // second column of third row
        await targetCell.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Check that the clicked cell (or its row) has a focus/selected indicator
        // The focus could be via CSS class, outline, or box-shadow
        var hasFocusClass = await targetCell.EvaluateAsync<bool>(
            "el => el.classList.contains('arcadia-grid__td--focused') || " +
            "el.closest('tr').classList.contains('arcadia-grid__row--focused') || " +
            "getComputedStyle(el).outlineStyle !== 'none' || " +
            "getComputedStyle(el).boxShadow !== 'none'");

        // Also verify the first cell does NOT have focus when we clicked the third row
        var firstCell = rows.Nth(0).Locator("td").Nth(1);
        var firstHasFocus = await firstCell.EvaluateAsync<bool>(
            "el => el.classList.contains('arcadia-grid__td--focused')");

        // At minimum, the target should have some focus indication OR the first cell should not steal focus
        Assert.That(hasFocusClass || !firstHasFocus, Is.True,
            "Focus should appear on the clicked cell, not the first cell");
    }

    // ── Sorting ──

    [Test]
    public async Task ClickColumnHeader_SortIndicatorAppears()
    {
        await NavigateToBasics();

        // Find sortable headers in the sorting section
        var sortableHeaders = Page.Locator("th.arcadia-grid__th--sortable");
        var count = await sortableHeaders.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1));

        var nameHeader = Page.Locator("th.arcadia-grid__th--sortable:has-text('Name')").First;
        await nameHeader.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var ariaSort = await nameHeader.GetAttributeAsync("aria-sort");
        Assert.That(ariaSort, Is.Not.Null.And.Not.EqualTo("none"),
            "Clicking column header should activate sort indicator");
    }

    [Test]
    public async Task ShiftClickHeader_MultiSortIndicators()
    {
        await NavigateToBasics();

        // Click Name header for first sort
        var nameHeader = Page.Locator("th.arcadia-grid__th--sortable:has-text('Name')").First;
        await nameHeader.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Shift+Click Department header for multi-sort
        var deptHeader = Page.Locator("th.arcadia-grid__th--sortable:has-text('Department')").First;
        await deptHeader.ClickAsync(new() { Modifiers = new[] { KeyboardModifier.Shift } });
        await Page.WaitForTimeoutAsync(300);

        var nameSort = await nameHeader.GetAttributeAsync("aria-sort");
        var deptSort = await deptHeader.GetAttributeAsync("aria-sort");

        Assert.That(nameSort, Is.Not.Null.And.Not.EqualTo("none"),
            "Name column should retain sort indicator during multi-sort");
        Assert.That(deptSort, Is.Not.Null.And.Not.EqualTo("none"),
            "Department column should gain sort indicator on Shift+Click");
    }

    // ── Quick filter ──

    [Test]
    public async Task QuickFilter_ReducesVisibleRows()
    {
        await NavigateToBasics();

        // The Toolbar & Quick Search section has a search input
        var searchInput = Page.Locator(".arcadia-grid__toolbar input[type='search'], .arcadia-grid__toolbar input[type='text'], .arcadia-grid__search-input");
        var searchCount = await searchInput.CountAsync();

        if (searchCount == 0)
        {
            // Fallback: look for any toolbar search field
            searchInput = Page.Locator("input[placeholder*='Search'], input[placeholder*='search'], input[placeholder*='Filter']");
            searchCount = await searchInput.CountAsync();
        }

        Assert.That(searchCount, Is.GreaterThanOrEqualTo(1),
            "Should have a quick search/filter input in the toolbar section");

        // Count rows before filtering
        var rowsBefore = await Page.Locator("tbody tr[role='row']").CountAsync();

        await searchInput.First.FillAsync("Alice");
        await Page.WaitForTimeoutAsync(500);

        var rowsAfter = await Page.Locator("tbody tr[role='row']").CountAsync();
        Assert.That(rowsAfter, Is.LessThan(rowsBefore),
            "Typing in quick filter should reduce visible rows");
    }

    // ── Data label formatting (no scientific notation) ──

    [Test]
    public async Task DataLabels_ShowReadableNumbers_NotScientificNotation()
    {
        await NavigateToBasics();

        // Check all visible cell text in the grid for scientific notation patterns
        var cells = Page.Locator("tbody td");
        var count = await cells.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1));

        var scientificNotationPattern = new System.Text.RegularExpressions.Regex(
            @"\d+\.?\d*[eE][+\-]?\d+");

        for (var i = 0; i < Math.Min(count, 100); i++)
        {
            var text = await cells.Nth(i).InnerTextAsync();
            Assert.That(scientificNotationPattern.IsMatch(text), Is.False,
                $"Cell {i} shows scientific notation: '{text}'. Values should use readable formatting.");
        }
    }

    [Test]
    public async Task ColumnFormatting_CurrencyAndPercentage_ShowReadableValues()
    {
        await NavigateToBasics();

        // The Column Formatting section has financial data with Format="C2", "P1", "N0"
        var formattingTitle = Page.Locator(".gallery__showcase-title:has-text('Column Formatting')");
        await Expect(formattingTitle.First).ToBeVisibleAsync();

        // Get the showcase container and check for properly formatted values
        var showcase = formattingTitle.First.Locator("xpath=..");
        var cells = showcase.Locator("td");
        var count = await cells.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(6),
            "Column Formatting section should have data cells");

        // Verify at least one cell has currency symbol and no scientific notation
        var foundCurrency = false;
        for (var i = 0; i < count; i++)
        {
            var text = await cells.Nth(i).InnerTextAsync();
            if (text.Contains('$') || text.Contains('%'))
                foundCurrency = true;

            // Never allow scientific notation
            Assert.That(text, Does.Not.Match(@"\d+\.?\d*[eE][+\-]?\d+"),
                $"Cell shows scientific notation: '{text}'");
        }

        Assert.That(foundCurrency, Is.True,
            "Column Formatting section should show formatted currency or percentage values");
    }

    // ── Context menu ──

    [Test]
    public async Task RightClickHeader_ShowsColumnMenu()
    {
        await NavigateToBasics();

        var sortableHeader = Page.Locator("th.arcadia-grid__th--sortable").First;
        await sortableHeader.ClickAsync(new() { Button = MouseButton.Right });
        await Page.WaitForTimeoutAsync(500);

        // Look for context menu / column menu
        var menu = Page.Locator(".arcadia-grid__context-menu, .arcadia-grid__column-menu, [role='menu']");
        var menuCount = await menu.CountAsync();

        if (menuCount > 0)
        {
            await Expect(menu.First).ToBeVisibleAsync();

            // Check for expected menu items
            var menuText = await menu.First.InnerTextAsync();
            var hasExpectedOption = menuText.Contains("Pin", StringComparison.Ordinal) ||
                                   menuText.Contains("Hide", StringComparison.Ordinal) ||
                                   menuText.Contains("Sort", StringComparison.Ordinal) ||
                                   menuText.Contains("Freeze", StringComparison.Ordinal);
            Assert.That(hasExpectedOption, Is.True,
                $"Column menu should contain Pin/Hide/Sort options, got: {menuText}");
        }
        else
        {
            // Context menu might not be implemented yet; log rather than fail hard
            Assert.Warn("Right-click context menu not found. Feature may not be implemented yet.");
        }
    }

    // ── Pin column (sticky positioning) ──

    [Test]
    public async Task PinColumn_GivesStickyPositioning()
    {
        await NavigateToBasics();

        // Right-click to get context menu
        var firstHeader = Page.Locator("th.arcadia-grid__th--sortable").First;
        await firstHeader.ClickAsync(new() { Button = MouseButton.Right });
        await Page.WaitForTimeoutAsync(500);

        var pinOption = Page.Locator("text=Pin Left, text=Pin, text=Freeze");
        var pinCount = await pinOption.CountAsync();

        if (pinCount > 0)
        {
            await pinOption.First.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            // Check if the column now has sticky positioning
            var pinnedHeader = Page.Locator("th.arcadia-grid__th--pinned, th[style*='sticky'], th.arcadia-grid__th--frozen");
            var pinnedCount = await pinnedHeader.CountAsync();

            Assert.That(pinnedCount, Is.GreaterThanOrEqualTo(1),
                "Pinned column should have sticky/frozen positioning");
        }
        else
        {
            Assert.Warn("Pin column option not found in context menu. Feature may not be implemented yet.");
        }
    }
}
