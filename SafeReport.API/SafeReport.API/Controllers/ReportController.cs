using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;
using SafeReport.Core.Models;
using System.Linq.Expressions;

namespace SafeReport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class ReportController(IReportService reportService) : ControllerBase
    {
        private readonly IReportService _reportService = reportService;


        [HttpPost("AddReport")]
        public async Task<IActionResult> AddReport([FromForm] CreateReportDto reportDto)
        {
            await _reportService.AddReportAsync(reportDto);
            return Ok(new { message = "Report created successfully" });
        }

        [HttpPost("GetAll")]
        public async Task<Response<PagedResultDto>> GetAll(ReportFilterDto? filter)
        {
            var result = await _reportService.GetPaginatedReportsAsync(filter);


            return Response<PagedResultDto>.SuccessResponse(result.Data, "Reports retrieved successfully");
        }
        [HttpGet("GetById/{id}")]
        public async Task<Response<ReportDto>> GetById(Guid id)
        {
            var result = await _reportService.GetReportByIdAsync(id);

            if (!result.Success)
                return Response<ReportDto>.FailResponse(result.Message);

            return Response<ReportDto>.SuccessResponse(result.Data, "Report retrieved successfully");
        }

        [HttpDelete("SoftDelete/{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var result = await _reportService.SoftDeleteReportAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("PrintReport/{id}")]
        public async Task<IActionResult> PrintReport(Guid id)
        {
            var pdfBytes = await _reportService.GetReportsPdfAsync(id);

            if (pdfBytes == null)
                return NotFound("Report not found.");

            return File(pdfBytes, "application/pdf", $"Report_{id}.pdf");
        }
        [HttpGet("GetNewReportsCount")]
        public async Task<ActionResult<Response<int>>> GetNewReportsCount(DateTime lastVisitUtc)
        {
            var count = await _reportService.GetNewReportsCount(lastVisitUtc);
            return Response<int>.SuccessResponse(count);
        }

    }
}
