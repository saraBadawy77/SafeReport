using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.Helper;
using SafeReport.Application.ISevices;
using SafeReport.Core.Interfaces;
using SafeReport.Core.Models;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;

namespace SafeReport.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IIncidentTypeRepository _incidentTypeRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<ReportHub> _hubContext;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReportService(
            IReportRepository reportRepository,
            IIncidentTypeRepository incidentTypeRepository,
            IMapper mapper,
            IHubContext<ReportHub> hubContext,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _reportRepository = reportRepository;
            _incidentTypeRepository = incidentTypeRepository;
            _mapper = mapper;
            _hubContext = hubContext;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<PagedResultDto>> GetPaginatedReportsAsync(ReportFilterDto? filter)
        {
            try
            {

                Expression<Func<Report, bool>> predicate = r => true;

                if (filter.IncidentId.HasValue && filter.CreatedDate.HasValue && filter.IncidentTypeId.HasValue)
                {
                    predicate = r => r.IncidentId == filter.IncidentId.Value &&
                                     r.CreatedDate.Date == filter.CreatedDate.Value.Date &&
                                     r.IncidentTypeId == filter.IncidentTypeId.Value;
                }
                else if (filter.IncidentId.HasValue)
                {
                    predicate = r => r.IncidentId == filter.IncidentId.Value;
                }
                else if (filter.CreatedDate.HasValue)
                {
                    predicate = r => r.CreatedDate.Date == filter.CreatedDate.Value.Date;
                }
                if (filter.IncidentTypeId.HasValue)
                    predicate = r => r.IncidentTypeId == filter.IncidentTypeId.Value;

                Expression<Func<Report, object>> include = r => r.Incident;
                // Pass to repository
                var reports = await _reportRepository.GetPagedAsync(
                    filter.PageNumber.Value,
                    filter.PageSize.Value,
                    predicate,
                    include);

                var totalCount = await _reportRepository.GetTotalCountAsync();
                // Get related incident type info
                var incidentTypeIds = reports.Select(r => r.IncidentTypeId).Distinct().ToList();
                var incidentIds = reports.Select(r => r.IncidentId).Distinct().ToList();

                var incidentTypes = await _incidentTypeRepository.FindAllAsync(
                    t => incidentIds.Contains(t.IncidentId) &&
                         incidentTypeIds.Contains(t.Id) &&
                         !t.IsDeleted);

                var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                // Prepare incident types dictionary
                var incidentTypeDict = incidentTypes.ToDictionary(
                    t => (t.IncidentId, t.Id),
                    t => currentCulture == "ar" ? t.NameAr ?? t.NameEn : t.NameEn ?? t.NameAr
                );

                // Map reports
                var reportDtos = reports.Select(report =>
                {
                    var description = currentCulture == "ar"
                        ? report.DescriptionAr ?? report.Description
                        : report.Description ?? report.DescriptionAr;

                    var incidentName = currentCulture == "ar"
                        ? report.Incident?.NameAr ?? report.Incident?.NameEn
                        : report.Incident?.NameEn ?? report.Incident?.NameAr;

                    incidentTypeDict.TryGetValue((report.IncidentId, report.IncidentTypeId), out string? incidentTypeName);

                    return new ReportDto
                    {
                        Id = report.Id,
                        Description = description,
                        CreatedDate = report.CreatedDate,
                        IncidentId = report.IncidentId,
                        IncidentName = incidentName ?? "N/A",
                        IncidentTypeId = report.IncidentTypeId,
                        IncidentTypeName = incidentTypeName ?? "N/A",
                        Address = report.Address,
                        Image = report.ImagePath,
                        PhoneNumber= report.PhoneNumber,
                        TimeSinceCreated = string.Empty
                    };
                }).ToList();

                // Wrap in paged result
                var pagedResult = new PagedResultDto
                {
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber.Value,
                    PageSize = filter.PageSize.Value,
                    Reports = reportDtos
                };

                return Response<PagedResultDto>.SuccessResponse(pagedResult, "Fetched reports successfully.");
            }
            catch (Exception ex)
            {
                return Response<PagedResultDto>.FailResponse($"Error: {ex.Message}");
            }
        }
        public async Task<Response<string>> SoftDeleteReportAsync(Guid id)
        {
            try
            {
                var report = await _reportRepository.FindAsync(r => r.Id == id);

                if (report == null)
                    return Response<string>.FailResponse($"Report with ID {id} not found.");

                _reportRepository.SoftDelete(report);
                _reportRepository.SaveChangesAsync();
                return Response<string>.SuccessResponse("Report soft deleted successfully.");
            }
            catch (Exception ex)
            {
                return Response<string>.FailResponse($"Error deleting report: {ex.Message}");
            }
        }
        public async Task<byte[]> GetReportsPdfAsync(Guid id)
        {
            Expression<Func<Report, object>>[] includes =
            {
                r => r.Incident
            };

            var report = await _reportRepository.FindAsync(r => r.Id == id, includes);


            var incidentType = await _incidentTypeRepository.FindAsync(t => t.Id == report.IncidentTypeId && t.IncidentId == report.IncidentId);
            if (report == null)
                return null;

            return PrintService.GenerateReportPdf(report, incidentType, _env);
        }
        public async Task<Response<string>> AddReportAsync(CreateReportDto reportDto)
        {
            try
            {
                var report = _mapper.Map<Report>(reportDto);

                // Get address from coordinates
                // report.Address = await GetAddressFromCoordinatesAsync(reportDto.Latitude, reportDto.Longitude);
                report.Address = "El Tahrir Square, Qasr Al Doubara, Bab al Luq, Cairo, 11519, Egypt";  // for test 

                if (reportDto.Image != null)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(reportDto.Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await reportDto.Image.CopyToAsync(stream);

                    report.ImagePath = $"images/{fileName}";
                }


                await _reportRepository.AddAsync(report);
                await _reportRepository.SaveChangesAsync();

                ReportDto reportdto = _mapper.Map<ReportDto>(report);
                await _hubContext.Clients.All.SendAsync("ReceiveNewReport", reportdto);

                return Response<string>.SuccessResponse("Report added successfully.");
            }
            catch (Exception ex)
            {
                return Response<string>.FailResponse($"Error adding report: {ex.Message}");
            }



        }
        private async Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using var httpClient = new HttpClient(handler);
                var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude}&lon={longitude}";


                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SafeReportApp/1.0");

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return "Unknown location";

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("display_name", out var displayName))
                    return displayName.GetString();

                return "Unknown location";
            }
            catch (Exception ex)
            {
                return $"Error adding report: {ex.Message}";
            }
        }
        public async Task<Response<ReportDto>> GetReportByIdAsync(Guid id)
        {
            try
            {
                Expression<Func<Report, object>>[] includes =
                   { r => r.Incident };
                var report = await _reportRepository.FindAsync(r => r.Id == id , includes);

                if (report == null)
                    return Response<ReportDto>.FailResponse("Report not found.");

                // Get related incident type info
                var incidentType = await _incidentTypeRepository.FindAsync(
                    t => t.Id == report.IncidentTypeId);

                var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                // Build localized fields
                string? description = currentCulture == "ar"
                    ? report.DescriptionAr ?? report.Description
                    : report.Description ?? report.DescriptionAr;

                string? incidentName = currentCulture == "ar"
                    ? report.Incident?.NameAr ?? report.Incident?.NameEn
                    : report.Incident?.NameEn ?? report.Incident?.NameAr;

                string? incidentTypeName = currentCulture == "ar"
                    ? incidentType?.NameAr ?? incidentType?.NameEn
                    : incidentType?.NameEn ?? incidentType?.NameAr;

                // Map to DTO
                var reportDto = new ReportDto
                {
                    Id = report.Id,
                    Description = description ?? "N/A",
                    CreatedDate = report.CreatedDate,
                    IncidentId = report.IncidentId,
                    IncidentName = incidentName ?? "N/A",
                    IncidentTypeId = report.IncidentTypeId,
                    IncidentTypeName = incidentTypeName ?? "N/A",
                    Address = report.Address ?? "N/A",
                    Image = ImageUrl.GetFullImageUrl(report.ImagePath, _httpContextAccessor),
                    PhoneNumber = report.PhoneNumber,
                    TimeSinceCreated = GetTimeSinceCreated(report.CreatedDate)
                };

                return Response<ReportDto>.SuccessResponse(reportDto, "Fetched report successfully.");
            }
            catch (Exception ex)
            {
                return Response<ReportDto>.FailResponse($"Error fetching report: {ex.Message}");
            }
        }

        private string GetTimeSinceCreated(DateTime createdDate)
        {
            var diff = DateTime.UtcNow - createdDate;
            if (diff.TotalMinutes < 60)
                return $"{Math.Floor(diff.TotalMinutes)} minutes ago";
            if (diff.TotalHours < 24)
                return $"{Math.Floor(diff.TotalHours)} hours ago";
            return $"{Math.Floor(diff.TotalDays)} days ago";
        }


        public async Task<int> GetNewReportsCount(DateTime lastVisitUtc)
        {
            Expression<Func<Report, bool>> filter = r => r.CreatedDate > lastVisitUtc;
            var count = await _reportRepository.CountAsync(filter);
            return count;
        }








    }
}





