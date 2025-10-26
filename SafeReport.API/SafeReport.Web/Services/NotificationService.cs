using Microsoft.AspNetCore.SignalR.Client;
using SafeReport.Web.DTOs;

namespace SafeReport.Web.Services
{
    public class NotificationService
    {
        private HubConnection? _hubConnection;
        public event Action<ReportDTO>? OnNewReport;

        public async Task StartAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7196/reportHub")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ReportDTO>("ReceiveNewReport", (report) =>
            {
                OnNewReport?.Invoke(report);
            });

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection failed: {ex.Message}");
            }
        }
    }
}
