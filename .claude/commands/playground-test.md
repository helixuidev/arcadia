---
description: Kill any running demo server, rebuild, start fresh, and visually verify every component category renders correctly using Playwright screenshots.
---

# Playground Verification — Arcadia Controls

Kill any running demo server, rebuild, start fresh, and verify every component category renders real content. Do NOT tell the user "it's ready" until every check passes.

IMPORTANT: Use Playwright screenshots to verify VISUAL correctness, not just HTTP status codes. Curl cannot catch broken colors, missing icons, or layout issues.

## Steps

### 1. Kill & Rebuild
- Kill any process on port 5050: `lsof -ti:5050 | xargs kill -9`
- Build the demo server: `dotnet build samples/Arcadia.Demo.Server/Arcadia.Demo.Server.csproj --framework net9.0`
- If build fails, FIX IT before continuing

### 2. Start Server
- `ASPNETCORE_URLS=http://localhost:5050 dotnet run --project samples/Arcadia.Demo.Server/Arcadia.Demo.Server.csproj --framework net9.0 &`
- Poll until `curl -s -o /dev/null -w "%{http_code}" http://localhost:5050` returns 200
- If it doesn't come up in 30 seconds, check build output for errors

### 3. Screenshot-Verify Sidebar
Take a Playwright screenshot of the sidebar and READ IT:
```javascript
await page.goto('http://localhost:5050', { waitUntil: 'networkidle' });
await page.screenshot({ path: '/tmp/verify-sidebar.png', clip: { x: 0, y: 0, width: 250, height: 800 } });
```
Check for:
- [ ] No gray/white browser default button backgrounds on section headers
- [ ] Icons visible next to section names (Charts, DataGrid, Forms, UI, Tools)
- [ ] Chevron toggles visible (▸ or ▾)
- [ ] Active page/tab highlighted
- [ ] No broken text, overlapping elements, or missing content

### 4. Verify Every Route Returns 200
Check ALL routes — not just the home page:
```bash
for route in / /datagrid/basics /datagrid/editing /datagrid/advanced /datagrid/enterprise; do
  code=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:5050$route")
  echo "$route: HTTP $code"
done
```
ALL must return 200. If any return 404, the @page directive is wrong or the file is missing.

### 5. Screenshot-Verify Every Section
Use Playwright to navigate to each section and take screenshots. READ each screenshot:
- Home page (product cards, stats, install block)
- Charts tab (Dashboard Widgets — KPI cards + sparklines should render)
- DataGrid Basics (/datagrid/basics — table with data visible)
- DataGrid Editing (/datagrid/editing)
- DataGrid Advanced (/datagrid/advanced — grouping visible)
- DataGrid Enterprise (/datagrid/enterprise — toolbar with CSV/Excel buttons)
- UI Components tab (Dialog button, Tabs, Tooltip, Cards, Badges)

For each, verify:
- [ ] Content renders (not blank)
- [ ] Colors match dark theme (no white/gray on dark background)
- [ ] Text is readable (correct contrast)
- [ ] No layout overflow or clipping

### 6. Verify Nav Links Work (Click-Through Test)
Use Playwright to click every sidebar link and verify it navigates correctly:
```javascript
// Click each nav link and check the page title changes
const links = await page.locator('a.gallery__nav-btn').all();
for (const link of links) {
  const href = await link.getAttribute('href');
  await link.click();
  await page.waitForTimeout(500);
  // Verify no 404 page
}
```

### 7. Verify WASM Demo
```bash
dotnet build samples/Arcadia.Demo.Wasm/Arcadia.Demo.Wasm.csproj
```
Must succeed with 0 errors. Also check WASM nav links use `/playground/` prefix for deployed URLs.

### 8. Run Unit Tests
```bash
dotnet test tests/Arcadia.Tests.Unit/Arcadia.Tests.Unit.csproj --framework net9.0 --filter "Category!=Performance" --verbosity quiet
```
Must show 0 failures.

### 9. Check Deployed URLs
If the WASM app deploys to arcadiaui.com/playground/, verify all DataGrid links use the `/playground/` prefix:
```bash
grep -rn 'href="/datagrid/' samples/Arcadia.Demo.Wasm/ --include="*.razor" | grep -v '/playground/datagrid/'
```
Any matches = BROKEN deployed links. Fix by prefixing with `/playground/`.

### 10. Report
Output a table:
| Check | Status |
|-------|--------|
| Build | PASS/FAIL |
| Server starts | PASS/FAIL |
| All routes 200 | PASS/FAIL |
| Sidebar visual | PASS/FAIL |
| Charts render | PASS/FAIL |
| DataGrid renders data | PASS/FAIL |
| UI Components render | PASS/FAIL |
| WASM builds | PASS/FAIL |
| WASM links correct | PASS/FAIL |
| Unit tests | PASS/FAIL (count) |

Only share the URL with the user AFTER all checks pass AND all screenshots look correct.

### Common Failures
1. **Gray section headers**: `<button>` elements need `background:transparent;border:none;color:inherit;`
2. **DataGrid no data**: Check `Collector.OnColumnsChanged` callback in `OnInitialized`
3. **404 on DataGrid pages**: Check `@page` directive matches the link `href`
4. **WASM deployed 404**: Links need `/playground/` prefix due to `<base href="/playground/">`
5. **Broken colors**: Check if CSS files are linked in App.razor / index.html (arcadia-core.css was missing once before)
