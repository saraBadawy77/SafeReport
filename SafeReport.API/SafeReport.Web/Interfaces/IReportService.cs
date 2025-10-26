using SafeReport.Web.DTOs;

namespace SafeReport.Web.Interfaces;

public interface IReportService
{
    Task<Response<PagedResultDto>> GetAllReportsAsync(ReportFilterDto filter);
    Task<bool> DeleteReportAsync(Guid id);
    Task<bool> PrintReportAsync(Guid id);
    Task<List<Response<IncidentType>>> GetAllIncidentsAsync();
    Task<Response<ReportDTO>> ShowReportDetails(Guid reportId);
    Task<int> GetNewReportsCountAsync(DateTime lastVisitUtc);


}
