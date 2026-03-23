using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Arcadia.Charts.Core;

/// <summary>
/// JS interop service for chart tooltips, export, resize, and pan/zoom.
/// Lazy-loads the chart-interop.js module on first use.
/// </summary>
public class ChartInteropService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public ChartInteropService(IJSRuntime js)
    {
        _js = js;
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Arcadia.Charts/js/chart-interop.js");
        return _module;
    }

    // ── Tooltip ──────────────────────────────────────

    /// <summary>Shows a tooltip near the cursor.</summary>
    public async ValueTask ShowTooltipAsync(string html, double x, double y)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("showTooltip", html, x, y);
    }

    /// <summary>Hides the tooltip.</summary>
    public async ValueTask HideTooltipAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("hideTooltip");
    }

    // ── Export ────────────────────────────────────────

    /// <summary>Exports the chart SVG as a string.</summary>
    public async ValueTask<string?> ExportSvgStringAsync(ElementReference container)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string?>("exportSvgAsString", container);
    }

    /// <summary>Exports chart as PNG and triggers download.</summary>
    public async ValueTask ExportPngAsync(ElementReference container, string filename = "chart.png", int scale = 2)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("exportAsPng", container, filename, scale);
    }

    /// <summary>Exports chart as SVG file and triggers download.</summary>
    public async ValueTask ExportSvgAsync(ElementReference container, string filename = "chart.svg")
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("exportAsSvg", container, filename);
    }

    // ── Resize ───────────────────────────────────────

    /// <summary>Observes container resize events.</summary>
    public async ValueTask ObserveResizeAsync<THandler>(ElementReference element, DotNetObjectReference<THandler> dotNetRef) where THandler : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("observeResize", element, dotNetRef);
    }

    /// <summary>Stops observing resize events.</summary>
    public async ValueTask UnobserveResizeAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("unobserveResize", element);
    }

    // ── Pan/Zoom ─────────────────────────────────────

    /// <summary>Enables pan and zoom on a chart container.</summary>
    public async ValueTask EnablePanZoomAsync<THandler>(ElementReference container, DotNetObjectReference<THandler> dotNetRef, string mode = "x") where THandler : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("enablePanZoom", container, dotNetRef, new { mode });
    }

    /// <summary>Disables pan and zoom.</summary>
    public async ValueTask DisablePanZoomAsync(ElementReference container)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("disablePanZoom", container);
    }

    /// <summary>Resets zoom to default.</summary>
    public async ValueTask ResetZoomAsync<THandler>(ElementReference container, DotNetObjectReference<THandler> dotNetRef) where THandler : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("resetZoom", container, dotNetRef);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try { await _module.DisposeAsync(); }
            #if NET6_0_OR_GREATER
            catch (JSDisconnectedException) { }
#else
            catch (Exception) { }
#endif
        }
    }
}

/// <summary>Handler for container resize events.</summary>
public interface IResizeHandler
{
    [JSInvokable]
    Task OnContainerResized(double width, double height);
}

/// <summary>Handler for pan/zoom events.</summary>
public interface IPanZoomHandler
{
    [JSInvokable]
    Task OnZoomChanged(double zoom, double centerX, double centerY);

    [JSInvokable]
    Task OnPanChanged(double offsetX, double offsetY);
}
