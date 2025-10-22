using SafeReport.UI.DTOs;
using SafeReport.UI.Interfaces;

namespace SafeReport.UI.Services;

public class ReportService: IReportService
{
    private readonly HttpClient _http;

    public ReportService(HttpClient http)
    {
        _http = http;
    }

    //public async Task<List<ReportDTO>> GetAllReportsAsync()
    //{
    //    var result = await _http.GetFromJsonAsync<List<ReportDTO>>("api/Reports/GetAll");
    //    return result ?? new List<ReportDTO>();
    //}

    //public async Task<bool> DeleteReportAsync(int id)
    //{
    //    var response = await _http.DeleteAsync($"api/Reports/Delete/{id}");
    //    return response.IsSuccessStatusCode;
    //}

    //public async Task PrintReportAsync(int id)
    //{
    //    var response = await _http.GetAsync($"api/Reports/Print/{id}");
    //    if (response.IsSuccessStatusCode)
    //    {
    //    }
    //}


    public async Task<List<ReportDTO>> GetAllReportsAsync()
    {
       
        await Task.Delay(500); 

        var dummyData = new List<ReportDTO>
            {
                new ReportDTO
                {
                    Id = 1,
                    ReportType = "Daily Summary",
                    Description = "Summary of today’s operations",
                    CreatedAt = DateTime.Now.AddHours(-3)
                },
                new ReportDTO
                {
                    Id = 2,
                    ReportType = "Monthly Performance",
                    Description = "Performance metrics for the current month",
                    CreatedAt = DateTime.Now.AddDays(-7)
                },
                new ReportDTO
                {
                    Id = 3,
                    ReportType = "System Errors",
                    Description = "List of critical system errors",
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };

        return dummyData;
    }

    public async Task<bool> DeleteReportAsync(int id)
    {
        await Task.Delay(200); // delay وهمي
        return true; // كأن الحذف تم بنجاح
    }

    public async Task PrintReportAsync(int id)
    {
        await Task.Delay(300); // delay وهمي
        Console.WriteLine($"Printing report #{id}...");
    }
}
