using System.Globalization;

namespace SafeReport.API.Extensions
{
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var defaultCulture = new CultureInfo("en-US");
            var cultureHeader = context.Request.Headers["Accept-Language"];

            CultureInfo culture;
            if (!string.IsNullOrWhiteSpace(cultureHeader))
            {
                try
                {

                    culture = new CultureInfo(cultureHeader);
                }
                catch
                {
                    culture = defaultCulture;
                }
            }
            else
            {
                culture = defaultCulture;
            }

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await _next(context);
        }
    }
}
