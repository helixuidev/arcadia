# Arcadia Controls IDE Snippets

Code snippet packs for quickly scaffolding Arcadia Controls chart components in Blazor projects.

## Available Snippets

| Shortcut     | Component                | Description                                      |
|--------------|--------------------------|--------------------------------------------------|
| `arcline`    | ArcadiaLineChart         | Line chart with series config                    |
| `arcbar`     | ArcadiaBarChart          | Bar chart with series config                     |
| `arcpie`     | ArcadiaPieChart          | Pie chart with name/value fields                 |
| `arcdonut`   | ArcadiaPieChart          | Donut chart (pie with InnerRadius)               |
| `arcscatter` | ArcadiaScatterChart      | Scatter chart with X/Y fields                    |
| `arcbubble`  | ArcadiaScatterChart      | Bubble chart (scatter with SizeField)            |
| `arccandle`  | ArcadiaCandlestickChart  | Candlestick chart with OHLC fields              |
| `arcradar`   | ArcadiaRadarChart        | Radar/spider chart with series                   |
| `arcgauge`   | ArcadiaGaugeChart        | Gauge with thresholds                            |
| `archeat`    | ArcadiaHeatmap           | Heatmap with X/Y/value fields                    |
| `arcfunnel`  | ArcadiaFunnelChart       | Funnel chart with name/value fields              |
| `arctree`    | ArcadiaTreemapChart      | Treemap with name/value fields                   |
| `arcwater`   | ArcadiaWaterfallChart    | Waterfall chart with category/value fields       |
| `arcrose`    | ArcadiaRoseChart         | Rose/polar area chart with name/value fields     |
| `arckpi`     | ArcadiaKpiCard           | KPI card with delta and sparkline                |
| `arcspark`   | ArcadiaSparkline         | Inline mini sparkline chart                      |
| `arcpage`    | Full Dashboard Page      | Complete Blazor page with KPIs + charts + data   |

## Installation

### Visual Studio

1. Open Visual Studio
2. Go to **Tools > Code Snippets Manager** (Ctrl+K, Ctrl+B)
3. Select **Language: HTML** from the dropdown
4. Click **Import...**
5. Navigate to `tools/snippets/vs/` and select all `.snippet` files
6. Click **Finish**

Alternatively, copy the `.snippet` files to your personal snippets folder:
- Windows: `%USERPROFILE%\Documents\Visual Studio 2022\Code Snippets\Visual Web Developer\My HTML Snippets\`

### JetBrains Rider

1. Open Rider
2. Go to **Settings > Editor > Live Templates**
3. Click the **Import** icon (or use **File > Manage IDE Settings > Import Settings**)
4. Select `tools/snippets/rider/arcadia-charts.xml`
5. Click **OK** to import the "Arcadia Controls" template group

Alternatively, copy `arcadia-charts.xml` to your Rider templates directory:
- macOS: `~/Library/Application Support/JetBrains/Rider<version>/templates/`
- Windows: `%APPDATA%\JetBrains\Rider<version>\templates\`
- Linux: `~/.config/JetBrains/Rider<version>/templates/`

## Usage

In a `.razor` file, type the shortcut (e.g., `arcline`) and press **Tab** (Visual Studio) or **Tab/Enter** (Rider) to expand the snippet. Use **Tab** to navigate between editable placeholders.
