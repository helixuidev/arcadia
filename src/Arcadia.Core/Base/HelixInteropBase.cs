using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Arcadia.Core.Base;

/// <summary>
/// Base class for HelixUI components that require JavaScript interop.
/// Handles lazy module loading and guaranteed disposal via <see cref="IAsyncDisposable"/>.
/// </summary>
public abstract class HelixInteropBase : HelixComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Gets the injected JS runtime for interop calls.
    /// </summary>
    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets the lazily-loaded JS module reference.
    /// </summary>
    protected IJSObjectReference? Module { get; private set; }

    /// <summary>
    /// Gets whether the JS module has been loaded.
    /// </summary>
    protected bool IsModuleLoaded => Module is not null;

    /// <summary>
    /// Gets the path to the JavaScript module file (e.g., "./_content/Arcadia.Core/js/focusTrap.js").
    /// </summary>
    protected abstract string ModulePath { get; }

    /// <summary>
    /// Loads the JavaScript module on first render after the component is connected to the DOM.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", ModulePath);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function on the loaded module.
    /// </summary>
    /// <param name="identifier">The function name to invoke.</param>
    /// <param name="args">Arguments to pass to the function.</param>
    protected async ValueTask InvokeModuleVoidAsync(string identifier, params object?[] args)
    {
        if (Module is not null)
        {
            await Module.InvokeVoidAsync(identifier, args);
        }
    }

    /// <summary>
    /// Invokes a JavaScript function on the loaded module and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="identifier">The function name to invoke.</param>
    /// <param name="args">Arguments to pass to the function.</param>
    protected async ValueTask<TResult> InvokeModuleAsync<TResult>(string identifier, params object?[] args)
    {
        if (Module is null)
        {
            return default!;
        }

        return await Module.InvokeAsync<TResult>(identifier, args);
    }

    /// <summary>
    /// Disposes the JS module reference.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override to perform additional async cleanup before the module is disposed.
    /// Always call <c>await base.DisposeAsyncCore()</c> at the end.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (Module is not null)
        {
            try
            {
                await Module.DisposeAsync();
            }
#if NET6_0_OR_GREATER
            catch (JSDisconnectedException)
            {
                // Circuit disconnected — safe to ignore
            }
#endif
            catch (Exception)
            {
                // On net5.0, JSDisconnectedException doesn't exist.
                // Swallow disposal errors when the circuit is gone.
            }

            Module = null;
        }
    }
}
