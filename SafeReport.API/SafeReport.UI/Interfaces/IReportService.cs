using SafeReport.UI.DTOs;

namespace SafeReport.UI.Interfaces;

public interface IReportService
{
    Task<List<ReportDTO>> GetAllReportsAsync();
    Task<bool> DeleteReportAsync(int id);
    Task PrintReportAsync(int id);
}
