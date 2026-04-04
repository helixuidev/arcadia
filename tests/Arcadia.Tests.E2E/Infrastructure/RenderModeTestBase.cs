using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Arcadia.Tests.E2E.Infrastructure;

/// <summary>
/// Base class for E2E tests that should run against both Server and WASM render modes.
/// <para>
/// Inherit from this class and use <c>[TestCaseSource(nameof(AllRenderModes))]</c> on
/// individual tests, or use <c>[ValueSource(nameof(AllRenderModes))]</c> as a parameter
/// source to run the same test against both demo apps.
/// </para>
/// <example>
/// <code>
/// [TestFixture]
/// public class MyDualModeTests : RenderModeTestBase
/// {
///     [Test]
///     public async Task HomePage_Returns200([ValueSource(nameof(AllRenderModes))] RenderMode mode)
///     {
///         var response = await Page.GotoAsync(BaseUrlFor(mode),
///             new() { WaitUntil = WaitUntilState.NetworkIdle });
///         Assert.That(response!.Status, Is.EqualTo(200));
///     }
/// }
/// </code>
/// </example>
/// </summary>
public class RenderModeTestBase : PageTest
{
    /// <summary>
    /// All render modes to test against. Use with [ValueSource] or [TestCaseSource].
    /// </summary>
    protected static readonly RenderMode[] AllRenderModes = { RenderMode.Server, RenderMode.Wasm };

    /// <summary>
    /// Returns the base URL for the given render mode.
    /// </summary>
    protected static string BaseUrlFor(RenderMode mode) => mode switch
    {
        RenderMode.Server => TestConstants.BaseUrl,
        RenderMode.Wasm => TestConstants.WasmBaseUrl,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    /// <summary>
    /// Navigates to a path on the demo app for the given render mode, waiting for network idle.
    /// </summary>
    protected async Task NavigateTo(RenderMode mode, string path)
    {
        var url = BaseUrlFor(mode) + path;
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }
}
