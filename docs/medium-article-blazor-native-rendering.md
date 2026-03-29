# Your Blazor Components Aren't Actually Blazor. And It's Costing You.

*Why the next generation of Blazor UI libraries renders in C#, not JavaScript*

---

I stumbled onto this almost by accident. I was debugging a Blazor Server app. Some Syncfusion chart wasn't rendering right after a parameter change, and I opened the browser DevTools expecting to see SVG elements I could trace back to my Razor code.

Instead I found a `<canvas>`. A JavaScript canvas. My "Blazor chart" was being drawn by EJ2, Syncfusion's JavaScript engine. My C# code was essentially a messenger, passing parameters across the interop bridge to a JS runtime I couldn't step through in Visual Studio.

I started digging. Telerik? Kendo UI underneath. DevExpress? DevExtreme JS. Even parts of Radzen wrap Chart.js for rendering. These are all solid products. I'm not bashing them. But it did make me wonder: if we chose Blazor to get away from JavaScript, why are we shipping megabytes of it anyway?

## The Costs Nobody Talks About

### The SSR flash that ruins first impressions

If you've worked with .NET 8's static server rendering, you know the pitch: server renders HTML instantly, ships it to the browser, then hydrates to interactive. Beautiful in theory.

Except JS-wrapper components can't participate in the server render. They emit an empty `<div>` placeholder during SSR, then the JavaScript loads, initializes the chart engine, and replaces the placeholder. Your users see a blank space, sometimes for a full second or two, before the chart pops in.

I honestly thought this was a Blazor problem for the longest time. It's not. It's a wrapper problem. Components that render SVG or HTML directly in `BuildRenderTree` ship the complete visual in the initial response. No flash. The chart is there on first paint.

Steve Sanderson (the creator of Blazor, for those who don't know) has talked about this. The framework is designed for server rendering, but components that punt to JS can't take advantage of it. It's a fundamental architectural mismatch that I don't think gets enough attention.

### Bundle size (and I mean, really)

I measured this because honestly I didn't believe it at first:

| Library | JS shipped to client |
|---------|---------------------|
| Syncfusion Blazor | ~3 MB |
| Telerik Blazor | ~2.5 MB |
| DevExpress Blazor | ~2 MB |
| Native C# approach | <12 KB |

Three. Megabytes. Of JavaScript. In a framework whose entire marketing pitch is "C# everywhere."

Now look, for an internal admin panel on a corporate network, nobody's going to notice. But I've been on projects where we're shipping to mobile users in Southeast Asia with spotty 3G, and 3 MB of JS before the page becomes interactive is... it's a problem. Daniel Roth from the Blazor team has specifically mentioned bundle size as a concern for WASM adoption, and here we are piling on megabytes of JS on top of the .NET runtime. I might be wrong about the exact numbers for your specific Syncfusion package configuration. Tree-shaking helps, but the order of magnitude difference is real.

### The memory thing (this one bit me personally)

I don't want to overstate this because it doesn't affect every project. But I need to mention it because it cost me a weekend.

Every JS interop call creates references on both sides. C# objects and JS objects, linked by an ID. The garbage collectors on each side don't coordinate. If a component with tooltips and hover effects makes 30-50 interop calls per second during user interaction, and the disposal lifecycle doesn't clean up perfectly every time...

I had a Blazor Server dashboard app climb to 400+ MB under sustained use. Six charts with real-time updates, some hover effects, nothing crazy. The "fix" was recycling the app pool every few hours. I don't love that we called that a fix.

Jon Hilton and Chris Sainty, both well-known in the Blazor community, have written about being careful with JS interop lifetime management. It's a known footgun. I think the less controversial version of my take here is: the fewer interop calls you make, the fewer ways things can go sideways.

### Debugging across two worlds

This one drives me genuinely nuts. A chart doesn't render right. Is the bug in my C# parameter mapping? In the JS wrapper's translation layer? In the underlying JS chart engine? I'm bouncing between Visual Studio, browser DevTools, and minified third-party JS I didn't write and can't read.

When a native SVG chart has a rendering bug, you put a breakpoint in `OnParametersSet`, step through the coordinate math, and see exactly where it goes wrong. One language. One debugger. One call stack. I know this sounds like a small thing but when you're two hours into a debugging session at 11pm, the simplicity matters.

### The Blazor Auto problem

This one I'm less sure about. I'd love to hear if others have had different experiences. .NET 8 introduced Auto render mode: start on the server, switch to WebAssembly when the runtime downloads. Brilliant for UX in theory.

But for JS-wrapper components, the switch can be rough. The component initializes on the server (no JS available), renders a placeholder, then when it moves to WASM, the JS engine needs to reinitialize everything from scratch. Some libraries handle this well. I've seen others where you get double-renders, flickering, or state that doesn't survive the transition.

Native components don't really have this problem. `BuildRenderTree` produces the same output regardless of where it runs. But I'll admit I haven't tested every library extensively in Auto mode, so take this one with some salt.

## What Does Native Rendering Actually Look Like?

Pipeline for a JS-wrapper gauge:

```
C# Parameter → JSON Serialize → SignalR/WASM bridge → JS Deserialize →
Canvas 2D API → Pixels
```

The native version:

```
C# Parameter → BuildRenderTree → SVG <path> → Browser GPU → Pixels
```

In practice, the C# code is just math. The kind of stuff we all learned in school and then forgot:

```csharp
var fraction = (Value - Min) / (Max - Min);
var angle = StartAngle + fraction * (EndAngle - StartAngle);
var x = cx + radius * Math.Cos(angle);
var y = cy + radius * Math.Sin(angle);
arcPath = $"M {start.X} {start.Y} A {radius} {radius} 0 {largeArc} 1 {x} {y}";
```

The output is an SVG `<path>` element. Real DOM. Participates in CSS transitions, responds to Blazor event handlers, works with `@ref`. The browser's GPU handles the painting, and it's been doing well for, what, 15 years now?

## The Part Where I'm Honest About the Downsides

I'd be a bad engineer if I didn't say this clearly: **JS wrapper libraries have more features and much larger communities.**

Syncfusion has 35+ chart types. Telerik's grid has been battle-tested in production by thousands of companies for 7+ years. They have actual support teams that answer the phone. There are hundreds of Stack Overflow answers. When you hit a weird edge case at 2am before a release, someone has probably hit it before you and posted the workaround.

Native rendering is newer. Way newer. The ecosystem is small. If you need a 3D surface plot or a Gantt chart with resource leveling, the JS-wrapper libraries are probably still your best (or only) bet. I'm not going to pretend otherwise.

I'm also not telling you to rip out Syncfusion from a working production app. That would be insane. I'm suggesting you think about the architecture for your *next* project, especially if SSR, bundle size, or Blazor Auto mode are important to you.

## If You're Curious

The easiest way to feel the difference is just to render something natively and compare. I work on [Arcadia Controls](https://arcadiaui.com) (full disclosure, yes I'm biased), and we recently extracted our gauge into a free standalone package. No dependencies on Arcadia.Core or anything else:

```bash
dotnet add package Arcadia.Gauge
```

```razor
@using Arcadia.Gauge.Components

<ArcadiaRadialGauge Value="73" Label="CPU Usage"
    ShowNeedle="true" ShowTicks="true" ShowTickLabels="true"
    GradientColors="@(new List<string> { "#22c55e", "#eab308", "#ef4444" })" />
```

MIT licensed. Under 15KB total. Works in Server, WASM, Auto. No config. The arc animation is pure CSS, so it actually plays during SSR prerender. Open DevTools and you'll see `<svg>` and `<path>` elements, not a canvas.

The full Arcadia suite has 20 chart types, a DataGrid, forms, and UI components if you want to go deeper. But honestly, just trying the gauge and inspecting what it renders in the DOM is enough to understand the architectural difference. It either clicks for you or it doesn't.

## Where I Think This Is Heading (Though I Could Be Wrong)

I think the trajectory is pretty clear, but I've been wrong before about where tech is going, so caveat emptor.

Blazor keeps getting better at server rendering, streaming, and render mode switching. Components that work *with* those features, not around them, will have an easier time. The JS-wrapper libraries know this too. Syncfusion has been investing in more Blazor-native rendering for newer components. Telerik's improving. The market pressure is real.

Maybe in two years every component library renders natively and this article looks quaint. That'd be great, honestly. The whole ecosystem benefits.

But right now? If your Blazor app ships megabytes of JavaScript to render components in a framework built to move past JavaScript... I don't know. It's at least worth asking the question.

---

**About me:** I've been building .NET apps for longer than I'd like to admit, and Blazor apps since the preview days when we were all trying to figure out if this thing was serious. (It was.) I work on [Arcadia Controls](https://arcadiaui.com), where we're betting on native C# rendering. It might be a bet that's too early. The ecosystem is young and we're competing against companies with hundred-person teams. But the technical argument feels right, even if the market hasn't caught up yet. The [Arcadia.Gauge](https://www.nuget.org/packages/Arcadia.Gauge) package is free forever if you want to kick the tires.

Find us on [GitHub](https://github.com/ArcadiaUIDev/arcadia). Or don't. I get that people are busy.
