using Microsoft.AspNetCore.Components;
using SafeReport.Web.DTOs;
using SafeReport.Web.Services;
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

        private string GetFullImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var baseUri = NavigationManager.BaseUri.TrimEnd('/');
            if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return relativePath;

            return $"{baseUri}/{relativePath.Replace("\\", "/")}";
        }

        private void GoBack()
        {
            NavigationManager.NavigateTo("/");
        }
    }
}
