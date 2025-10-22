using Microsoft.AspNetCore.Mvc;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;

namespace SafeReport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
