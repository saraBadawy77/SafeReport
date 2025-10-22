using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;

namespace SafeReport.Web.Extensions
{
    public static class CultureExtensions
    {
        public  async static Task EnsureDefaultCultureAsync(this WebAssemblyHost host)
        {
            var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();

            var existingCulture = await jsRuntime.InvokeAsync<string>("blazorCulture.get");
            CultureInfo culture;
            if (existingCulture !=null)
            {
                culture=new CultureInfo(existingCulture);
            }
            else
            {
                culture=new CultureInfo("en-US");
            }
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Console.WriteLine(culture);
        }
    }
}
