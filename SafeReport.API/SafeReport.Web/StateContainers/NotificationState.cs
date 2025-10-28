namespace SafeReport.Web.StateContainers
{
    using Microsoft.JSInterop;
    using SafeReport.Web.DTOs;
    using SafeReport.Web.Services;
    using System.Collections.Generic;
    using System.Text.Json;
    public class NotificationState
    {
        private readonly IJSRuntime _js;
        private const string StorageKey = "NewReports";

        public NotificationState(IJSRuntime js)
        {
            _js = js;
        }

        public List<ReportDTO> NewReports { get; private set; } = new();
        public int NewReportsCount => NewReports.Count;
        public event Action? OnChange;

        public async Task InitializeAsync()
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var savedReports = JsonSerializer.Deserialize<List<ReportDTO>>(json);
                if (savedReports != null)
                    NewReports = savedReports;
            }
            NotifyStateChanged();
        }

        public async Task AddReportAsync(ReportDTO report)
        {
            if (!NewReports.Exists(r => r.Id == report.Id))
            {
                NewReports.Add(report);
                await SaveAsync();
                NotifyStateChanged();
            }
        }

        public async Task RemoveReportAsync(Guid id)
        {
            NewReports.RemoveAll(r => r.Id == id);
            await SaveAsync();
            NotifyStateChanged();
        }

        public async Task ClearReportsAsync()
        {
            NewReports.Clear();
            await SaveAsync();
            NotifyStateChanged();
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(NewReports);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
  