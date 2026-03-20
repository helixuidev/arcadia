using System.Threading;

namespace HelixUI.Core.Utilities;

/// <summary>
/// Generates unique, deterministic IDs suitable for HTML element identification
/// and ARIA attribute linking.
/// </summary>
public static class IdGenerator
{
    private static long _counter;

    /// <summary>
    /// Generates a unique ID with the default "helix-" prefix.
    /// Thread-safe and guaranteed unique within the application lifetime.
    /// </summary>
    public static string Generate()
    {
        return Generate("helix");
    }

    /// <summary>
    /// Generates a unique ID with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix for the generated ID (e.g., "helix-input").</param>
    public static string Generate(string prefix)
    {
        var id = Interlocked.Increment(ref _counter);
        return $"{prefix}-{id}";
    }

    /// <summary>
    /// Resets the counter. Intended for testing only.
    /// </summary>
    internal static void Reset()
    {
        Interlocked.Exchange(ref _counter, 0);
    }
}
