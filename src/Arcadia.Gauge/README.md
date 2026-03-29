<p align="center">
  <strong>Arcadia.Gauge</strong><br>
  <em>Free, standalone Blazor gauge components — radial, semi-circular, and linear. Pure SVG, zero JavaScript.</em>
</p>

## Why Arcadia Gauge?

- **Free forever** — MIT licensed, no watermark, no Pro lock
- **Zero dependencies** — doesn't require Arcadia.Core or any other package
- **Tiny** — under 15KB total (DLL + CSS)
- **Pure SVG** — renders on the server, no JS hydration
- **Accessible** — WCAG 2.1 AA, screen reader support

## Features

| Feature | Status |
|---------|--------|
| Circular gauge (full 360°) | Yes |
| Semi-circular gauge (180°) | Yes |
| Custom start/end angles | Yes |
| Needle pointer | Yes |
| Tick marks (major + minor) | Yes |
| Tick labels | Yes |
| Color thresholds | Yes |
| Gradient arcs | Yes |
| Multiple colored ranges | Yes |
| Animated transitions | Yes |
| Center template | Yes |
| Rounded arc caps | Yes |
| Responsive sizing | Yes |

## Quick Start

```bash
dotnet add package Arcadia.Gauge
```

```html
<link href="_content/Arcadia.Gauge/css/arcadia-gauge.css" rel="stylesheet" />
```

```razor
@using Arcadia.Gauge.Components

<ArcadiaRadialGauge Value="73" Label="CPU Usage" />

<ArcadiaRadialGauge Value="850" Min="300" Max="900" Label="Score"
                    FullCircle="true" ShowNeedle="true"
                    ShowTicks="true" ShowTickLabels="true" />
```

## Part of Arcadia Controls

Arcadia.Gauge is a standalone free package. For charts, DataGrid, forms, and more, check out [Arcadia Controls](https://arcadiaui.com).

**[Documentation](https://arcadiaui.com/docs/charts/gauge-chart)** · **[Live Demo](https://arcadiaui.com/playground/)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**
