# Arcadia.DashboardKit

Drag-and-drop dashboard grid layout for Blazor with smooth FLIP animations, spring physics, and touch support.

## Features

- **Drag & reorder** panels with pointer events (mouse, touch, pen)
- **FLIP animations** for smooth layout transitions
- **2D occupancy grid** for correct placement of mixed-size panels
- **iOS wiggle mode** — long-press to enter edit mode with jiggling cards
- **Resize panels** with grid-snapping
- **Locked items** that can't be moved
- **State persistence** to localStorage
- **Keyboard accessible** — Tab/Enter/Arrows/Escape
- **Add/remove panels** dynamically
- **Floating panels** for overlapping content
- Works in **Blazor Server, WebAssembly, and Auto** render modes

## Quick Start

```bash
dotnet add package Arcadia.DashboardKit
```

```razor
<ArcadiaDragGrid Columns="4" RowHeight="140" Gap="16">
    <ArcadiaDragGridItem Id="revenue" ColSpan="2">
        <p>Revenue: $142,580</p>
    </ArcadiaDragGridItem>
    <ArcadiaDragGridItem Id="users">
        <p>Active Users: 8,429</p>
    </ArcadiaDragGridItem>
    <ArcadiaDragGridItem Id="chart" ColSpan="2" RowSpan="2">
        <ArcadiaLineChart Data="@data" ... />
    </ArcadiaDragGridItem>
</ArcadiaDragGrid>
```

## iOS Wiggle Mode

```razor
<ArcadiaDragGrid DragMode="longpress" LongPressDuration="500">
    ...
</ArcadiaDragGrid>
```

Long-press any card for 500ms to enter edit mode. All cards jiggle. Drag to reorder. Press "Done" to exit.

## License

Part of [Arcadia Controls](https://arcadiaui.com). Pro license required for commercial use.
