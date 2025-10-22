using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SafeReport.UI.Components;
using SafeReport.UI.Interfaces;
using SafeReport.UI.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

#region 🔧 Configure Services

// ✅ Localization setup
builder.Services.AddLocalization();

// ✅ Add Razor & Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ✅ Register custom services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHttpClient<IReportService, ReportService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7196/");
});

#endregion

var app = builder.Build();


var supportedCultures = new[] {"ar-EG", "en-US"};

var Localization= new RequestLocalizationOptions()
    .SetDefaultCulture("ar-EG")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(Localization);






if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapBlazorHub();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
