# DataGrid Phase 3 + Spreadsheet — Mini PRP

## Strategic Decision: DataGrid vs Spreadsheet

These are **two different products** serving different users:

| | DataGrid | Spreadsheet |
|---|---|---|
| **Mental model** | Database table viewer | Excel in the browser |
| **Data flow** | Read-mostly, API-backed | Edit-first, local data |
| **Cell editing** | Double-click one cell, commit | Always editable, Tab between cells |
| **Selection** | Row-based | Cell-range (A1:C5) |
| **Formulas** | No | Yes (=SUM, =IF, =VLOOKUP) |
| **Cell types** | Uniform per column | Mixed per cell |
| **Undo/redo** | Per-edit | Full history stack |
| **Copy/paste** | Row-level | Cell-range clipboard (TSV) |
| **Sizing** | Rows × columns from data | Fixed grid (100 rows × 26 cols) |
| **Target user** | Developer building an admin panel | Business user replacing Excel |
| **Competition** | Syncfusion Grid, Telerik Grid | Syncfusion Spreadsheet, Handsontable, AG Grid |
| **Package** | `Arcadia.DataGrid` | `Arcadia.Spreadsheet` (separate) |

**Decision: Build them separately.** The DataGrid gets cell editing as a feature. The Spreadsheet is a future standalone package with a fundamentally different architecture (cell-addressed model, formula engine, clipboard manager).

---

## DataGrid Phase 3: What we're building now

### 1. Cell Editing (inline)
**The feature that makes the grid interactive.**

Implementation:
- `Editable="true"` on ArcadiaColumn enables editing
- `EditTemplate` RenderFragment<TItem> for custom edit UI
- Default edit: auto-generates `<input>` based on Field return type
- Enter edit: double-click cell (or press Enter when focused)
- Commit: Enter/Tab → fires `OnRowEdit` EventCallback
- Cancel: Escape → reverts to display mode
- Only one cell edits at a time (single-edit mode)

State model:
- `_editingRow: TItem?` — the row being edited
- `_editingColumn: string?` — the column key being edited
- Edit values stored in a `Dictionary<string, object?>` to allow rollback

### 2. Grouping
**Visually groups rows by a column value.**

Implementation:
- `Groupable="true"` on grid enables grouping
- `GroupBy` string parameter — column key to group by
- Alternative: click column header context menu → "Group by this"
- Renders group header rows with value + count
- Expand/collapse groups (all expanded by default)
- Group headers use a `GroupHeaderTemplate` or auto-format

Data flow:
```
Data → Filter → Sort → Group → Page → Render
```

Group structure:
```csharp
internal class GroupedData<TItem>
{
    public object GroupValue { get; set; }
    public string GroupLabel { get; set; }
    public List<TItem> Items { get; set; }
    public bool IsExpanded { get; set; } = true;
}
```

### 3. Excel/CSV Export
**Enterprise must-have.**

Implementation:
- No third-party dependency — generate CSV natively, Excel via OpenXML-compatible minimal writer
- Export respects current sort + filter state
- `ExportCsv()` and `ExportExcel()` methods on the grid (callable from toolbar or code)
- Uses JS interop for file download (Blob → URL.createObjectURL → anchor click)
- Toolbar gets "Export" dropdown: CSV, Excel

CSV generation:
```csharp
public string ToCsv()
{
    var sb = new StringBuilder();
    // Header row
    sb.AppendLine(string.Join(",", visibleColumns.Select(c => Quote(c.Title))));
    // Data rows (filtered + sorted, all pages)
    foreach (var item in GetFilteredSortedData())
        sb.AppendLine(string.Join(",", visibleColumns.Select(c => Quote(c.FormatValue(c.Field(item))))));
    return sb.ToString();
}
```

For Excel: generate a minimal .xlsx (ZIP of XML files) without any third-party library. The format is:
- `[Content_Types].xml`
- `xl/workbook.xml`
- `xl/worksheets/sheet1.xml` — the actual cell data
- `xl/sharedStrings.xml` — string table
- `_rels/.rels` and `xl/_rels/workbook.xml.rels`

This is ~200 lines of code for a basic Excel file.

### 4. Keyboard Navigation
**Accessibility requirement and power-user feature.**

Implementation:
- Arrow keys navigate between cells
- Enter activates sort on header / starts edit on body cell
- Escape cancels edit
- Tab moves to next editable cell
- Home/End jump to first/last column
- Page Up/Down change pages
- `tabindex="0"` on the grid container
- `@onkeydown` handler on the grid div

---

## Implementation Order

1. **Cell editing** — highest user impact, most complex
2. **Export (CSV)** — quick win, very useful
3. **Grouping** — visual wow factor
4. **Keyboard navigation** — polish

---

## Spreadsheet: Future Package Scope (NOT building now)

For reference, here's what `Arcadia.Spreadsheet` would eventually need:

| Feature | Complexity |
|---------|-----------|
| Cell-addressed model (A1, B2) | Medium |
| Formula engine (=SUM, =IF, =VLOOKUP, 50+ functions) | Very High |
| Cell formatting (bold, colors, borders per cell) | Medium |
| Cell-range selection (click+drag, Shift+arrow) | High |
| Clipboard (copy/paste cell ranges as TSV) | Medium |
| Undo/redo stack | Medium |
| Freeze rows/columns | Medium |
| Merge cells | Medium |
| Auto-fill (drag handle) | High |
| Sheet tabs (multi-sheet) | Medium |
| CSV/Excel import | Medium |
| Excel export | Already built in DataGrid |
| Virtual scrolling (1M cells) | High |

**Estimated effort: 4-6 weeks for MVP (cell model + formulas + selection + clipboard).**

This is a separate initiative and should not block DataGrid Phase 3.

---

## Files to create/modify (Phase 3)

| File | Action |
|------|--------|
| `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor.cs` | ADD — edit state, group logic, export methods, keyboard handler |
| `src/Arcadia.DataGrid/Components/ArcadiaDataGrid.razor` | ADD — edit mode cells, group headers, export toolbar, keyboard events |
| `src/Arcadia.DataGrid/Core/GroupedData.cs` | NEW — group data structure |
| `src/Arcadia.DataGrid/Core/ExcelExporter.cs` | NEW — minimal .xlsx writer |
| `src/Arcadia.DataGrid/wwwroot/js/datagrid-interop.js` | NEW — file download via Blob |
| `src/Arcadia.DataGrid/wwwroot/css/arcadia-datagrid.css` | ADD — edit mode, group header, keyboard focus styles |
| `samples/.../TestDataGrid.razor` | UPDATE — show editing + grouping + export |

## Success Criteria

- Double-click a salary cell → input appears → type new value → Enter → `OnRowEdit` fires
- Click "Export CSV" → downloads filtered data as .csv file
- Group by Department → see department headers with row counts
- Arrow keys navigate cells, Enter starts edit
