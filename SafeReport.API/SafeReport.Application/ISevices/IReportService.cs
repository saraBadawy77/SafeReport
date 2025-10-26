using Microsoft.AspNetCore.Mvc;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;

namespace SafeReport.Application.ISevices
{
    public interface IReportService
    {
        Task<Response<PagedResultDto>> GetPaginatedReportsAsync(ReportFilterDto? filter);
        Task<Response<string>> SoftDeleteReportAsync(Guid id);
        Task<byte[]> GetReportsPdfAsync(Guid id);
        Task<Response<string>> AddReportAsync(CreateReportDto reportDto);
        Task<Response<ReportDto>> GetReportByIdAsync(Guid id);
        Task<int> GetNewReportsCount(DateTime lastVisitUtc);
    }
}
