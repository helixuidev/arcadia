# DataGrid Keyboard Navigation — PRP

## Problem

Keyboard-only users cannot interact with the grid. No arrow key navigation, no Enter to activate, no Escape to cancel. This is a WCAG 2.1 AA violation and a blocker for enterprise procurement.

## Scope

Full keyboard navigation following the WAI-ARIA Grid pattern (https://www.w3.org/WAI/ARIA/apg/patterns/grid/).

## Behavior Spec

### Navigation Keys

| Key | Context | Action |
|-----|---------|--------|
| `ArrowDown` | Any cell | Move focus to same column, next row |
| `ArrowUp` | Any cell | Move focus to same column, previous row |
| `ArrowRight` | Any cell | Move focus to next column, same row |
| `ArrowLeft` | Any cell | Move focus to previous column, same row |
| `Home` | Any cell | Move focus to first column, same row |
| `End` | Any cell | Move focus to last column, same row |
| `Ctrl+Home` | Any cell | Move focus to first cell (row 1, col 1) |
| `Ctrl+End` | Any cell | Move focus to last cell (last row, last col) |
| `PageDown` | Any cell | Go to next page, keep column focus |
| `PageUp` | Any cell | Go to previous page, keep column focus |
| `Tab` | Any cell | Move to next interactive element (exit grid) |

### Action Keys

| Key | Context | Action |
|-----|---------|--------|
| `Enter` | Header cell | Toggle sort on that column |
| `Enter` | Body cell (editable) | Start inline edit |
| `Enter` | Edit input | Commit edit, move focus to next row |
| `Escape` | Edit input | Cancel edit, restore focus to cell |
| `Space` | Row (selectable) | Toggle row selection |
| `Space` | Expand cell | Toggle detail row expand/collapse |
| `Ctrl+A` | Anywhere in grid | Select all rows (if MultiSelect) |

### Focus Management

- Grid container gets `tabindex="0"` for initial focus entry
- Active cell tracked by `(_focusRow, _focusCol)` indices
- Visual focus indicator: 2px accent ring inside the cell (not outline — stays within grid bounds)
- When grid receives focus, restore last focused cell or default to (0, 0)
- Focus ring moves via CSS class `.arcadia-grid__td--focused`
- Screen reader: `aria-activedescendant` on the grid element pointing to the focused cell's ID

## Implementation

### State

```csharp
private int _focusRow = 0;
private int _focusCol = 0;
private bool _gridHasFocus;
```

### Event Handler

Single `@onkeydown` on the grid container div. Switch on `e.Key`:

```csharp
internal void HandleGridKeyDown(KeyboardEventArgs e)
{
    switch (e.Key)
    {
        case "ArrowDown": MoveFocus(1, 0); break;
        case "ArrowUp": MoveFocus(-1, 0); break;
        case "ArrowRight": MoveFocus(0, 1); break;
        case "ArrowLeft": MoveFocus(0, -1); break;
        case "Home": _focusCol = e.CtrlKey ? 0 : 0; if (e.CtrlKey) _focusRow = 0; break;
        case "End": _focusCol = MaxCol; if (e.CtrlKey) _focusRow = MaxRow; break;
        case "PageDown": GoToPage(_pageIndex + 1); break;
        case "PageUp": GoToPage(_pageIndex - 1); break;
        case "Enter": HandleEnterKey(); break;
        case "Escape": CancelEdit(); break;
        case " ": HandleSpaceKey(); break;
    }
    e.PreventDefault(); // for arrow keys to not scroll the page
}
```

### CSS

```css
.arcadia-grid__td--focused {
  box-shadow: inset 0 0 0 2px var(--_accent);
}
.arcadia-grid:focus-within .arcadia-grid__td--focused {
  box-shadow: inset 0 0 0 2px var(--_accent);
}
```

### Accessibility Attributes

```html
<div role="grid" tabindex="0"
     aria-activedescendant="@($"cell-{_focusRow}-{_focusCol}")"
     @onkeydown="HandleGridKeyDown"
     @onfocus="@(() => _gridHasFocus = true)"
     @onblur="@(() => _gridHasFocus = false)">
```

Each cell gets an ID: `id="@($"cell-{rowIdx}-{colIdx}")"`.

## Files to Modify

| File | Changes |
|------|---------|
| `ArcadiaDataGrid.razor.cs` | Add focus state, HandleGridKeyDown, MoveFocus |
| `ArcadiaDataGrid.razor` | Add tabindex, onkeydown, aria-activedescendant, cell IDs, focus class |
| `arcadia-datagrid.css` | Add .arcadia-grid__td--focused style |

## Testing

- E2E: Arrow keys move focus indicator
- E2E: Enter on header sorts
- E2E: Space toggles selection
- E2E: Escape cancels edit
- E2E: Tab exits grid
- Manual: screen reader announces cell content on focus
