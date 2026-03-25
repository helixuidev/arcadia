# DataGrid ARIA Live Regions — PRP

## Problem

Screen reader users are not informed when grid content changes due to sort, filter, page change, or selection. They interact with a control but get no feedback.

## Solution

Add an `aria-live="polite"` region that announces state changes.

## Announcements

| Action | Announcement |
|--------|-------------|
| Sort change | "Sorted by {column}, {ascending/descending}" |
| Filter applied | "{count} results found" |
| Filter cleared | "Filter cleared, showing all {count} rows" |
| Page change | "Page {n} of {total}, showing rows {start} to {end}" |
| Row selected | "{name} selected" |
| Row deselected | "{name} deselected" |
| Select all | "{count} rows selected" |
| Detail expanded | "Details expanded for row {n}" |
| Detail collapsed | "Details collapsed for row {n}" |
| Edit started | "Editing {column} in row {n}" |
| Edit committed | "Changes saved" |
| Edit cancelled | "Edit cancelled" |
| Export | "Exported {count} rows to CSV" |

## Implementation

### Razor

```razor
<div class="arcadia-sr-only" role="status" aria-live="polite" aria-atomic="true">
    @_liveAnnouncement
</div>
```

### Code-behind

```csharp
private string? _liveAnnouncement;

private void Announce(string message)
{
    _liveAnnouncement = message;
    StateHasChanged();
    // Clear after a tick so the same message can be re-announced
    _ = Task.Delay(100).ContinueWith(_ => {
        _liveAnnouncement = null;
        InvokeAsync(StateHasChanged);
    });
}
```

Call `Announce()` in each action handler:
- `HandleHeaderClick()` → `Announce($"Sorted by {column.Title}, {direction}")`
- `SetFilter()` → `Announce($"{TotalCount} results")`
- `GoToPage()` → `Announce($"Page {_pageIndex + 1} of {PageCount}")`
- `ToggleSelection()` → `Announce($"{item} {selected/deselected}")`

## Files to Modify

| File | Changes |
|------|---------|
| `ArcadiaDataGrid.razor` | Add sr-only live region div |
| `ArcadiaDataGrid.razor.cs` | Add _liveAnnouncement, Announce(), calls in handlers |

## Testing

- E2E: Verify aria-live region content changes on sort
- E2E: Verify announcement text matches filter count
- Manual: Test with VoiceOver / NVDA
