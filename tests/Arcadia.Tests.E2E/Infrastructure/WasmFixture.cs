using System.Diagnostics;
using System.Net;

namespace Arcadia.Tests.E2E.Infrastructure;

[SetUpFixture]
public class WasmFixture
{
    private Process? _wasmProcess;

    [OneTimeSetUp]
    public async Task StartWasmHost()
    {
        // Check if WASM host is already running (e.g. in CI or manual start)
        if (await IsWasmRunning())
        {
            Console.WriteLine("WASM demo already running at " + TestConstants.WasmBaseUrl);
            return;
        }

        Console.WriteLine("Starting WASM demo host...");
        var projectPath = FindProjectPath();

        _wasmProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --framework net9.0 --no-launch-profile",
                Environment =
                {
                    ["ASPNETCORE_URLS"] = TestConstants.WasmBaseUrl,
                    ["ASPNETCORE_ENVIRONMENT"] = "Development"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _wasmProcess.Start();

        // WASM apps can take longer to start (framework files must be served)
        var timeout = TimeSpan.FromSeconds(90);
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (await IsWasmRunning())
            {
                Console.WriteLine($"WASM demo host ready after {sw.Elapsed.TotalSeconds:F1}s");
                return;
            }
            await Task.Delay(1000);
        }

        throw new TimeoutException($"WASM demo host did not start within {timeout.TotalSeconds}s");
    }

    [OneTimeTearDown]
    public void StopWasmHost()
    {
        if (_wasmProcess is not null && !_wasmProcess.HasExited)
        {
            _wasmProcess.Kill(entireProcessTree: true);
            _wasmProcess.Dispose();
            Console.WriteLine("WASM demo host stopped.");
        }
    }

    private static async Task<bool> IsWasmRunning()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync(TestConstants.WasmBaseUrl);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private static string FindProjectPath()
    {
        // Walk up from test assembly to find the repo root
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "samples", "Arcadia.Demo.Wasm", "Arcadia.Demo.Wasm.csproj");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Could not find Arcadia.Demo.Wasm.csproj. Run tests from the repo root.");
    }
}
