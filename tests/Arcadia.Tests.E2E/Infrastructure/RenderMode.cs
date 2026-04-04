namespace Arcadia.Tests.E2E.Infrastructure;

/// <summary>
/// Identifies which render mode (Server or WASM) a test is running against.
/// Used by <see cref="RenderModeTestBase"/> to parameterize tests across both demo apps.
/// </summary>
public enum RenderMode
{
    Server,
    Wasm
}
