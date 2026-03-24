# Product Hunt Launch — Arcadia Controls

## Tagline (60 chars max)
Blazor charts: 14 types, pure SVG, zero JavaScript, 244KB

## Description (260 chars max)
Arcadia Controls is a lightweight Blazor chart library with 14 chart types — all pure SVG, no JavaScript runtime. At 244KB total, it's a fraction of the size and cost of Telerik or Syncfusion. Includes an AI-powered MCP server that generates chart code from plain English.

## Maker Comment (~200 words, first comment on PH listing)

Hey Product Hunt! I'm excited to share Arcadia Controls — a Blazor charting library I built out of frustration.

I've spent years building enterprise .NET dashboards, and every charting library I tried had the same problems: massive JavaScript bundles, accessibility afterthoughts, and price tags that made my finance team cry. Telerik wants $1,249/yr. Syncfusion wants $995. And both ship megabytes of JS that fight with Blazor's rendering model.

So I built something different. Arcadia renders everything as native SVG — no JavaScript runtime, no WebGL, no canvas hacks. The entire library is 244KB. It works across .NET 5 through .NET 10, supports Server, WASM, and Auto render modes, and every chart is WCAG 2.1 AA accessible out of the box.

The part I'm most proud of: our MCP server integration. You can tell Claude or Copilot "create a bar chart showing quarterly revenue" and it generates production-ready Blazor code using the Arcadia API. AI-assisted chart development, built into the library.

We have a free Community tier with 4 chart types. Pro starts at $299/yr — that's 76% less than the competition.

I'd love your feedback. What charts would you build first?

## 5 Key Features to Highlight

1. **14 Chart Types** — Line, Bar, Area, Pie, Donut, Scatter, Radar, Funnel, Heatmap, Treemap, Gauge, Sparkline, Rose, Waterfall
2. **244KB Total Bundle** — Pure SVG rendering, zero JavaScript runtime dependencies
3. **AI Code Generation** — Built-in MCP server lets AI assistants generate chart code from natural language descriptions
4. **WCAG 2.1 AA Accessible** — Every chart ships with proper ARIA labels, keyboard navigation, and screen reader support
5. **Multi-framework Support** — Works on .NET 5-10, Blazor Server, WebAssembly, and Auto render modes

## Suggested Topics/Tags for Product Hunt

- Developer Tools
- Open Source
- .NET
- Data Visualization
- Artificial Intelligence
- SaaS
- Web Development
