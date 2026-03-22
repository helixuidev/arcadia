using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Arcadia.Demo.Wasm;
using Arcadia.Theme;
using Arcadia.Notifications;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ToastService>();

await builder.Build().RunAsync();
