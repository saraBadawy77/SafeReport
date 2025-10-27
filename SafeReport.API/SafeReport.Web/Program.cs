using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SafeReport.Web;
using SafeReport.Web.Extensions;
using SafeReport.Web.Interfaces;
using SafeReport.Web.Services;
using SafeReport.Web.StateContainers;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// Register your custom services
builder.Services.AddScoped<ReportService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<NotificationState>();

// Register HttpClient for WebAssembly
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7196/") });
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddLocalization();
var host = builder.Build();
await host.SetDefaultCulture();
await host.RunAsync();
await builder.Build().RunAsync();
