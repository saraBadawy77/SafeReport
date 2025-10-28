﻿using AutoMapper;
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
using SafeReport.Infrastructure.Repositories;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;

namespace SafeReport.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IIncidentTypeRepository _incidentTypeRepository;
        private readonly IIncidentRepository _incidentRepository ;
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
            IHttpContextAccessor httpContextAccessor,
            IIncidentRepository incidentRepository)
        {
            _reportRepository = reportRepository;
            _incidentTypeRepository = incidentTypeRepository;
            _mapper = mapper;
            _hubContext = hubContext;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _incidentRepository = incidentRepository;
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

                // Fetch paged reports with related entities
                var reports = await _reportRepository.GetPagedAsync(
                    filter.PageNumber.Value,
                    filter.PageSize.Value,
                    predicate,
                    r => r.CreatedDate,
                    descending: true,
                    r => r.Incident,
                    r => r.Images);

                var totalCount = await _reportRepository.GetTotalCountAsync(predicate);

                // Prepare incident types dictionary
                var incidentTypeIds = reports.Select(r => r.IncidentTypeId).Distinct().ToList();
                var incidentIds = reports.Select(r => r.IncidentId).Distinct().ToList();

                var incidentTypes = await _incidentTypeRepository.FindAllAsync(
                    t => incidentIds.Contains(t.IncidentId) &&
                         incidentTypeIds.Contains(t.Id) &&
                         !t.IsDeleted);

                var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                var incidentTypeDict = incidentTypes.ToDictionary(
                    t => (t.IncidentId, t.Id),
                    t => currentCulture == "ar" ? t.NameAr ?? t.NameEn : t.NameEn ?? t.NameAr
                );

                // Map reports
                var reportDtos = reports.Select(report => MapReportToDto(report, incidentTypeDict, currentCulture)).ToList();

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

        #region Report Mapping 

        private ReportDto MapReportToDto(Report report, Dictionary<(int IncidentId, int Id), string> incidentTypeDict, string currentCulture)
        {
            var description = currentCulture == "ar"
                ? report.DescriptionAr ?? report.Description
                : report.Description ?? report.DescriptionAr;

            var incidentName = currentCulture == "ar"
                ? report.Incident?.NameAr ?? report.Incident?.NameEn
                : report.Incident?.NameEn ?? report.Incident?.NameAr;

            incidentTypeDict.TryGetValue((report.IncidentId, report.IncidentTypeId), out string? incidentTypeName);

            var images = report.Images?
                .Where(img => !string.IsNullOrEmpty(img.ImagePath))
                .Select(img => img.ImagePath)
                .ToList() ?? new List<string>();

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
                ImagePaths = images,
                PhoneNumber = report.PhoneNumber,
                TimeSinceCreated = string.Empty
            };
        }

        #endregion

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
                r => r.Incident,
                r=>r.Images
            };

            var report = await _reportRepository.FindAsync(r => r.Id == id, includes);


            var incidentType = await _incidentTypeRepository.FindAsync(t => t.Id == report.IncidentTypeId && t.IncidentId == report.IncidentId);
            if (report == null)
                return null;

            return PrintService.GenerateReportPdf(report, incidentType, _env);
        }
        private async Task<List<ReportImage>> SaveReportImagesAsync(List<IFormFile>? images)
        {
            var savedImages = new List<ReportImage>();
            if (images == null || !images.Any())
                return savedImages;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in images)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedImages.Add(new ReportImage
                    {
                        ImagePath = $"images/{fileName}",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            return savedImages;
        }
        public async Task<Response<string>> AddReportAsync(CreateReportDto reportDto)
        {
            try
            {
                var incidentExists = await _incidentRepository.GetByIdAsync(reportDto.IncidentId);
                if (incidentExists is null)
                    return Response<string>.FailResponse("Invalid IncidentId.");

                var incidentType = await _incidentTypeRepository.GetByIdAsync(reportDto.IncidentTypeId);
                if (incidentType == null)
                    return Response<string>.FailResponse("Incident type not found");

                var selectedIncidentType = await _incidentTypeRepository.FindAsync(
                    t => t.IncidentId == reportDto.IncidentId &&
                         t.Id == reportDto.IncidentTypeId &&
                         !t.IsDeleted);
                if (selectedIncidentType is null)
                    return Response<string>.FailResponse("The selected incident type does not belong to the chosen incident.");

                var report = _mapper.Map<Report>(reportDto);
                        // Get address from coordinates
             // report.Address = await GetAddressFromCoordinatesAsync(reportDto.Latitude, reportDto.Longitude);
                report.Address = "El Tahrir Square, Qasr Al Doubara, Bab al Luq, Cairo, 11519, Egypt";  // for test
                string currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                if (reportDto.Images != null && reportDto.Images.Any())
                {
                    report.Images = await SaveReportImagesAsync(reportDto.Images);
                }

                await _reportRepository.AddAsync(report);
                await _reportRepository.SaveChangesAsync();

                var incidentTypeDict = (await _incidentTypeRepository.GetAllAsync())
                    .Where(t => !t.IsDeleted)
                    .ToDictionary(t => (t.IncidentId, t.Id),
                                  t => currentCulture == "ar" ? t.NameAr : t.NameEn);

                var reportDtoResult = MapnewReportToDto(report, incidentTypeDict, currentCulture);

                // Notify clients
                await _hubContext.Clients.All.SendAsync("ReceiveNewReport", reportDtoResult);

                return Response<string>.SuccessResponse("Report added successfully.");
            }
            catch (Exception ex)
            {
                return Response<string>.FailResponse($"Error adding report: {ex.Message}");
            }
        }

        private ReportDto MapnewReportToDto(Report report, Dictionary<(int IncidentId, int Id), string> incidentTypeDict, string currentCulture)
        {
            var description = currentCulture == "ar"
                ? report.DescriptionAr ?? report.Description
                : report.Description ?? report.DescriptionAr;

            var incidentName = currentCulture == "ar"
                ? report.Incident?.NameAr ?? report.Incident?.NameEn
                : report.Incident?.NameEn ?? report.Incident?.NameAr;

            incidentTypeDict.TryGetValue((report.IncidentId, report.IncidentTypeId), out string? incidentTypeName);

            var images = report.Images?
                .Where(img => !string.IsNullOrEmpty(img.ImagePath))
                .Select(img => img.ImagePath)
                .ToList() ?? new List<string>();

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
                ImagePaths = images,
                PhoneNumber = report.PhoneNumber,
                TimeSinceCreated = string.Empty  
            };
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
                { r => r.Incident, r => r.Images };
                var report = await _reportRepository.FindAsync(r => r.Id == id, includes);

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

                // Map images to full URLs
                var imageUrls = report.Images?
                    .Where(img => !string.IsNullOrEmpty(img.ImagePath))
                    .Select(img => ImageUrl.GetFullImageUrl(img.ImagePath, _httpContextAccessor))
                    .ToList() ?? new List<string>();

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
                    ImagePaths = imageUrls,
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
        public async Task<Response<List<ReportDto>>> GetNewReports(DateTime lastVisitUtc)
        {
            try
            {
                // Filter reports created after the user's last visit
                Expression<Func<Report, bool>> filter = r => r.CreatedDate > lastVisitUtc;

                var reports = await _reportRepository.FindAllIncludes(
                    filter,
                    r => r.Incident,
                    r => r.Images
                );
                // Get related incident types
                var incidentTypeIds = reports.Select(r => r.IncidentTypeId).Distinct().ToList();
                var incidentIds = reports.Select(r => r.IncidentId).Distinct().ToList();

                var incidentTypes = await _incidentTypeRepository.FindAllAsync(
                    t => incidentIds.Contains(t.IncidentId) &&
                         incidentTypeIds.Contains(t.Id) &&
                         !t.IsDeleted);
                
                var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                // Build dictionary for incident type names (localized)
                var incidentTypeDict = incidentTypes.ToDictionary(
                    t => (t.IncidentId, t.Id),
                    t => currentCulture == "ar"
                        ? t.NameAr ?? t.NameEn
                        : t.NameEn ?? t.NameAr
                );

                // Map all reports to DTOs
                var reportDtos = reports.Select(report =>
                    MapReportToDto(report, incidentTypeDict, currentCulture)
                ).ToList();

                return Response<List<ReportDto>>.SuccessResponse(reportDtos, $"Found {reportDtos.Count} new reports.");
            }
            catch (Exception ex)
            {
                return Response<List<ReportDto>>.FailResponse($"Error fetching new reports: {ex.Message}");
            }
        }













    }
}





