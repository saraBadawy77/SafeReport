using SafeReport.Web.DTOs;

namespace SafeReport.Web.Interfaces;

public interface IReportService
{
    Task<Response<PagedResultDto>> GetAllReportsAsync(ReportFilterDto filter);
    Task<bool> DeleteReportAsync(Guid id);
    Task<bool> PrintReportAsync(Guid id);
    Task<List<Response<Incident>>> GetAllIncidentsAsync();
    Task<Response<ReportDTO>> ShowReportDetails(Guid reportId);
    //Task<int> GetNewReportsCountAsync(DateTime lastVisitUtc);
    Task<Response<List<ReportDTO>>> GetNewReportsAsync(DateTime lastVisitUtc);
    Task<Response<List<IncidentTypeDto>>> GetAllIncidentTypeAsync();
    Task<Response<List<IncidentTypeDto>>> GetIncidentTypesByIncidentIdAsync(int incidentId);




}
