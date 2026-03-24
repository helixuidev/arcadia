# Social Media Copy — Arcadia Controls Product Hunt Launch

## Twitter/X Post (280 chars)

We just launched Arcadia Controls on @ProductHunt!

14 Blazor chart types. Pure SVG. Zero JavaScript. 244KB total.

Plus an AI-powered MCP server that generates chart code from plain English.

Free tier available. Pro is $299/yr (vs $1,249 for Telerik).

https://producthunt.com/posts/arcadia-controls

## LinkedIn Post (~300 words)

**We just launched Arcadia Controls on Product Hunt — and I want to tell you why it matters for .NET developers.**

For years, building data visualizations in Blazor meant choosing between two painful options: expensive enterprise suites with massive JavaScript bundles, or cobbling together open-source JS libraries with fragile interop layers.

Today we're changing that with Arcadia Controls.

**What makes it different:**

Arcadia renders all 14 chart types as pure SVG — directly in Blazor's render tree. There is no JavaScript runtime. No WebGL. No canvas fallbacks. The entire library weighs 244KB. Compare that to the multi-megabyte bundles that Telerik and Syncfusion ship.

This matters because Blazor's strength is keeping logic in C#. Every JS interop call is a performance tax. We eliminated that tax entirely.

**The AI angle:**

We built a Model Context Protocol (MCP) server into the library. Connect it to Claude, Copilot, or any MCP-compatible AI assistant, and you can generate production-ready chart code from natural language. Say "create a heatmap of user activity by hour and day" and get working Blazor markup.

**Accessibility is not optional:**

Every Arcadia chart ships WCAG 2.1 AA compliant. Proper ARIA labels. Keyboard navigation. Screen reader support. Because enterprise software needs to work for everyone.

**Pricing that respects your budget:**

- Community: Free forever (4 chart types, MIT licensed)
- Pro: $299/yr per developer
- Enterprise: $799/yr with priority support

That is 76% less than Telerik ($1,249) and 70% less than Syncfusion ($995).

We would love your support on Product Hunt today, and your honest feedback on what we can improve.

Link in comments.

#dotnet #blazor #csharp #dataviz #producthunt #opensource

## Reddit r/dotnet Post

**Title:** We launched Arcadia Controls — 14 Blazor chart types, pure SVG, zero JavaScript (244KB total)

**Body:**

Hey r/dotnet,

We just launched Arcadia Controls on Product Hunt and wanted to share it with this community.

**The problem:** Charting in Blazor usually means either paying $1,000+/yr for Telerik/Syncfusion (with heavy JS interop) or hacking together Chart.js wrappers that break on every update.

**Our approach:** Arcadia renders all charts as native SVG directly in Blazor's component tree. No JavaScript runtime at all. The full library is 244KB.

**What you get:**

- 14 chart types: Line, Bar, Area, Pie, Donut, Scatter, Radar, Funnel, Heatmap, Treemap, Gauge, Sparkline, Rose, Waterfall
- Works on .NET 5 through .NET 10 (Server, WASM, Auto)
- WCAG 2.1 AA accessible out of the box
- Built-in MCP server for AI-assisted chart code generation
- Free Community tier (4 chart types, MIT licensed)
- Pro at $299/yr per developer

**Quick example:**

```razor
<ArcBarChart
    Title="Revenue by Quarter"
    Data="@quarters"
    XField="Quarter"
    YField="Revenue"
    Animate="true" />
```

We are genuinely looking for feedback. What chart features matter most to you? What are the pain points in your current charting setup?

Product Hunt link: https://producthunt.com/posts/arcadia-controls
Docs: https://arcadiaui.com/docs
Playground: https://arcadiaui.com/playground/

## Reddit r/Blazor Post

**Title:** Arcadia Controls: 14 chart types rendered as pure SVG — no JS interop needed

**Body:**

Hey r/Blazor,

I have been building Blazor apps for years and always struggled with charting. JS interop wrappers for Chart.js or D3 felt wrong — they fight Blazor's render model and add complexity.

So I built Arcadia Controls. Every chart renders as SVG directly in Blazor's component tree. No JavaScript module loading. No IJSRuntime calls. No serialization overhead. Just C# parameters and Blazor rendering.

**Why this matters for Blazor specifically:**

- Charts participate in Blazor's diffing algorithm — only changed elements re-render
- No JS interop means no async overhead on every data update
- Works identically in Server, WASM, and .NET 8+ Auto render modes
- Pre-rendering works perfectly since there is no JS to hydrate

**14 chart types available:** Line, Bar, Area, Pie, Donut, Scatter, Radar, Funnel, Heatmap, Treemap, Gauge, Sparkline, Rose, Waterfall

**Other highlights:**
- 244KB total package size
- WCAG 2.1 AA accessible
- .NET 5-10 multi-targeting
- MCP server for AI code generation (works with Claude, Copilot)
- Free Community tier with 4 chart types

Would love feedback from the Blazor community. What rendering edge cases should I test? Any chart types you want that we are missing?

Docs: https://arcadiaui.com/docs/charts
Playground: https://arcadiaui.com/playground/

## Hacker News Title

Launch HN: Arcadia Controls – Blazor chart library, pure SVG, no JS runtime (244KB)
