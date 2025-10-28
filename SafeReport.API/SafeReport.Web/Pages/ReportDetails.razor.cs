using Microsoft.AspNetCore.Components;
using SafeReport.Web.DTOs;
using SafeReport.Web.Services;
using System.Globalization;
using static System.Net.WebRequestMethods;

namespace SafeReport.Web.Pages
{
    public partial class ReportDetails
    {
        [Parameter]
        public Guid ReportId { get; set; }

        private ReportDTO? report;
        private bool isLoading = true;
        private bool loadFailed = false;
        [Inject]
        private ReportService ReportService { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var response = await ReportService.ShowReportDetails(ReportId);
                if (response != null && response.Success)
                {
                    report = response.Data;
                }
                else
                {
                    loadFailed = true;
                }
            }
            catch
            {
                loadFailed = true;
            }
            finally
            {
                isLoading = false;
            }
        }
        private string GetCultureClass()
        {
            return CultureInfo.CurrentCulture.Name switch
            {
                "ar-EG" => "rtl",
                "en-US" => "ltr",
                _ => "ltr"
            };
        }
        private string GetFullImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var baseUri = NavigationManager.BaseUri.TrimEnd('/');
            if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return relativePath;

            return $"{baseUri}/{relativePath.Replace("\\", "/")}";
        }
        private string TranslateTimeSinceCreated(string englishText)
        {
            // englishText = "3 minutes ago" أو "2 hours ago" أو "1 days ago"
            var parts = englishText.Split(' '); // ["3", "minutes", "ago"]

            if (parts.Length >= 3)
            {
                var value = parts[0];
                var unit = parts[1];

                return unit switch
                {
                    "minutes" => string.Format(_Localizer["MinutesAgo"], value),
                    "minute" => string.Format(_Localizer["MinuteAgo"], value),
                    "hours" => string.Format(_Localizer["HoursAgo"], value),
                    "hour" => string.Format(_Localizer["HourAgo"], value),
                    "days" => string.Format(_Localizer["DaysAgo"], value),
                    "day" => string.Format(_Localizer["DayAgo"], value),
                    _ => englishText
                };
            }

            return englishText;
        }


        private void GoBack()
        {
            NavigationManager.NavigateTo("/reports");
        }
    }
}
