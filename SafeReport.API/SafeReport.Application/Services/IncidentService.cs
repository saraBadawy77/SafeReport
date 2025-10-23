using AutoMapper;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;
using SafeReport.Core.Interfaces;
using SafeReport.Core.Models;
using System.Globalization;

public class IncidentService(IIncidentRepository incidentRepository, IIncidentTypeRepository incidentTypeRepository, IMapper mapper) : IIncidentService
{
    private readonly IIncidentRepository _incidentRepository = incidentRepository;
    private readonly IIncidentTypeRepository _incidentTypeRepository = incidentTypeRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Response<IEnumerable<IncidentDto>>> GetAllAsync()
    {
        try
        {
            var items = await _incidentRepository.GetAllAsync();
            var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var result = items.Select(item => new IncidentDto
            {
                Id = item.Id,
                Name = currentCulture == "ar" ? item.NameAr : item.NameEn,
                Description = currentCulture == "ar" ? item.DescriptionAr : item.DescriptionEn,
                Creationdate = item.CreationDate
            }).ToList();
            return Response<IEnumerable<IncidentDto>>.SuccessResponse(result, "Fetched incidents successfully.");
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<IncidentDto>>.FailResponse($"Error: {ex.Message}");
        }
    }

    public async Task<Response<List<IncidentTypeDto?>>> GetIncidentType(int incidentId)
    {
        try
        {
            // Fetch all incident types matching the provided ID and not deleted
            var incidents = await _incidentTypeRepository.FindAllAsync(t => t.IncidentId == incidentId && !t.IsDeleted);

            if (incidents == null || !incidents.Any())
                return Response<List<IncidentTypeDto?>>.FailResponse("Incident not found.");

            // Map to DTO list
            var dtoList = _mapper.Map<List<IncidentTypeDto?>>(incidents);

            return Response<List<IncidentTypeDto?>>.SuccessResponse(dtoList, "Incident(s) found successfully.");
        }
        catch (Exception ex)
        {
            return Response<List<IncidentTypeDto?>>.FailResponse($"Error: {ex.Message}");
        }
    }

    public async Task<Response<IncidentDto>> CreateAsync(CreateIncidentDto dto)
    {
        try
        {
            var incident = _mapper.Map<Incident>(dto);
            await _incidentRepository.AddAsync(incident);
            await _incidentRepository.SaveChangesAsync();
            var incidentDto = _mapper.Map<IncidentDto>(incident);
            return Response<IncidentDto>.SuccessResponse(incidentDto, "Incident created successfully.");
        }
        catch (Exception ex)
        {
            return Response<IncidentDto>.FailResponse($"Creation failed: {ex.Message}");
        }
    }

    public async Task<Response<bool>> UpdateAsync(int id, CreateIncidentDto dto)
    {
        try
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null || incident.IsDeleted)
                return Response<bool>.FailResponse("Incident not found.");

            _mapper.Map(dto, incident);

            _incidentRepository.Update(incident);
            await _incidentRepository.SaveChangesAsync();

            return Response<bool>.SuccessResponse(true, "Incident updated successfully.");
        }
        catch (Exception ex)
        {
            return Response<bool>.FailResponse($"Update failed: {ex.Message}");
        }
    }

    public async Task<Response<bool>> SoftDeleteAsync(int id)
    {
        try
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null || incident.IsDeleted)
                return Response<bool>.FailResponse("Incident not found.");

            _incidentRepository.SoftDelete(incident);
            await _incidentRepository.SaveChangesAsync();

            return Response<bool>.SuccessResponse(true, "Incident deleted successfully.");
        }
        catch (Exception ex)
        {
            return Response<bool>.FailResponse($"Delete failed: {ex.Message}");
        }
    }
}
