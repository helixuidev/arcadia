using Arcadia.Demo.Server.Components;
using Arcadia.Theme;
using Arcadia.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



builder.Services.AddScoped<ThemeService>(_ => new ThemeService(new DarkTheme()));
builder.Services.AddScoped<ToastService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Security headers middleware
// CSP tuned for Blazor Server compatibility:
//   - script-src needs 'unsafe-inline' because Blazor Server's _framework/blazor.server.js
//     bootstrap uses inline <script> tags; nonce-based CSP would require custom infrastructure.
//   - connect-src needs ws:/wss: for the SignalR WebSocket circuit.
//   - cdnjs.cloudflare.com is allowed for dev-tool scripts (axe-core a11y auditing in E2E tests).
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'wasm-unsafe-eval' https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self'; " +
        "img-src 'self' data:; " +
        "connect-src 'self' ws: wss:");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
