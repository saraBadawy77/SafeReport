using AutoMapper;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;
using SafeReport.Core.Interfaces;
using SafeReport.Core.Models;
using SafeReport.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Application.Services
{
    public class IncidentTypeService(IIncidentTypeRepository incidentTypeRepository, IMapper mapper, IIncidentRepository  incidentRepository) : IIncidentTypeService
    {
        private readonly IIncidentTypeRepository _incidentTypeRepository = incidentTypeRepository;
        private readonly IIncidentRepository _incidentRepository = incidentRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<IEnumerable<IncidentTypeDto>>> GetAllAsync()
        {
            try
            {
                var items = await _incidentTypeRepository.GetAllAsync();

                var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                var result = items.Select(item => new IncidentTypeDto
                {
                    Id = item.Id,
                    Name = currentCulture == "ar" ? item.NameAr : item.NameEn,
                    Description = currentCulture == "ar" ? item.DescriptionAr : item.DescriptionEn,
                    Creationdate = item.CreationDate
                }).ToList();

                return Response<IEnumerable<IncidentTypeDto>>.SuccessResponse(result, "Fetched incidents successfully.");
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<IncidentTypeDto>>.FailResponse($"Error: {ex.Message}");
            }
        }

        public async Task<Response<List<IncidentTypeDto?>>> GetIncidentType(int incidentId)
        {
            try
            {
                // Fetch all incident types matching the provided ID and not deleted
                var incidents = await _incidentTypeRepository.FindAllAsync(t => t.IncidentId == incidentId);

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

        public async Task<Response<IncidentTypeDto>> CreateAsync(CreateIncidentTypeDto incidentTypeDto)
        {
            try
            {
                var incidentExists =await _incidentRepository.GetByIdAsync(incidentTypeDto.IncidentId);
                if (incidentExists is  null)
                    return Response<IncidentTypeDto>.FailResponse("Invalid IncidentId.");
                var incident = _mapper.Map<IncidentType>(incidentTypeDto);
                await _incidentTypeRepository.AddAsync(incident);
                await _incidentTypeRepository.SaveChangesAsync();
                var incidentDto = _mapper.Map<IncidentTypeDto>(incident);
                return Response<IncidentTypeDto>.SuccessResponse(incidentDto, "Incident created successfully.");
            }
            catch (Exception ex)
            {
                return Response<IncidentTypeDto>.FailResponse($"Creation failed: {ex.Message}");
            }
        }

        public async Task<Response<bool>> UpdateAsync(int id, CreateIncidentTypeDto incidentTypeDto)
        {
            try
            {
                var incidentType = await _incidentTypeRepository.GetByIdAsync(id);
                if (incidentType == null || incidentType.IsDeleted)
                    return Response<bool>.FailResponse("Incident type not found.");

                var incidentExists = await _incidentRepository.GetByIdAsync(incidentTypeDto.IncidentId);
                if (incidentExists is null)
                    return Response<bool>.FailResponse("Invalid IncidentId.");

                _mapper.Map(incidentTypeDto, incidentType);

                _incidentTypeRepository.Update(incidentType);
                await _incidentTypeRepository.SaveChangesAsync();

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
                var incident = await _incidentTypeRepository.GetByIdAsync(id);
                if (incident == null || incident.IsDeleted)
                    return Response<bool>.FailResponse("Incident not found.");

                _incidentTypeRepository.SoftDelete(incident);
                await _incidentTypeRepository.SaveChangesAsync();

                return Response<bool>.SuccessResponse(true, "Incident deleted successfully.");
            }
            catch (Exception ex)
            {
                return Response<bool>.FailResponse($"Delete failed: {ex.Message}");
            }
        }
    }
}
