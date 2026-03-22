# CLAUDE.md — HelixUI DashboardKit (Data Visualization Specialist Context)

## Role: Data Visualization & Dashboard Specialist

You own the DashboardKit — drag-and-drop dashboards, chart integration, and real-time data presentation.

## Responsibilities
- Build the dashboard layout engine (drag, drop, resize widgets)
- Integrate charting (ApexCharts or custom SVG)
- Handle real-time data via SignalR
- Export dashboards (PDF, PNG, Excel)
- Make widget layouts serializable and persistable

## DashboardKit Architecture
```
HelixUI.DashboardKit/
├── wwwroot/js/
│   └── helix-dashboard.ts       # Drag/resize interop (Gridstack.js or custom)
├── Components/
│   ├── HelixDashboard.razor      # Main dashboard container
│   ├── HelixWidget.razor         # Base widget wrapper
│   ├── WidgetToolbar.razor       # Widget header with actions (move, resize, remove, config)
│   ├── DashboardEditor.razor     # Edit mode overlay (add widgets, change layout)
│   └── Widgets/
│       ├── ChartWidget.razor     # Chart container (line, bar, pie, area, donut)
│       ├── StatWidget.razor      # Single KPI stat card
│       ├── TableWidget.razor     # Mini data table
│       ├── ListWidget.razor      # Item list (tasks, alerts, etc.)
│       ├── TextWidget.razor      # Rich text / markdown content
│       └── CustomWidget.razor    # Render fragment for consumer-defined widgets
├── Models/
│   ├── DashboardLayout.cs        # Serializable layout (positions, sizes)
│   ├── WidgetConfig.cs           # Widget configuration model
│   ├── ChartConfig.cs            # Chart-specific options
│   └── DashboardTheme.cs         # Dashboard-level theme overrides
├── Services/
│   ├── DashboardStateService.cs  # Layout persistence (save/load/reset)
│   ├── RealTimeDataService.cs    # SignalR hub connection for live data
│   └── ExportService.cs          # PDF/PNG/Excel export
└── HelixUI.DashboardKit.csproj
```

## Design Rules
1. **Layout is serializable** — `DashboardLayout` is JSON-friendly, save/load per user
2. **Widget API is extensible** — consumers define custom widgets via `RenderFragment` or component type
3. **Real-time first** — SignalR integration built in, not bolted on
4. **Responsive** — dashboards reflow on mobile (stack to single column)
5. **Edit vs View mode** — clear separation, edit mode shows grid lines and handles
6. **Theme-aware charts** — charts use HelixUI design tokens, adapt to dark mode
7. **Export clean** — PDF/PNG export renders a clean version (no edit handles)

## Drag & Drop Strategy
- Use CSS Grid for layout positioning
- Gridstack.js via JS interop for drag/resize (proven, lightweight)
- Serialize positions as grid coordinates (col, row, width, height)
- Minimum widget size: 2x2 grid cells
- Snap to grid, no free-form positioning

## Chart Integration
- Wrap ApexCharts.js (MIT, well-maintained, responsive)
- C# model → JSON options → JS render
- Support: Line, Bar, Area, Pie, Donut, Radar, Heatmap, Treemap
- Real-time append: push new data points without full re-render

## Performance Targets
- Dashboard load (10 widgets): < 500ms
- Widget drag/resize: 60fps
- Chart update (single data point): < 16ms
- Full layout save: < 100ms
- Export to PDF: < 3s for 10-widget dashboard
