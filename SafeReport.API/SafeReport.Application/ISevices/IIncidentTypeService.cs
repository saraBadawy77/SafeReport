using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Application.ISevices
{
    public interface IIncidentTypeService
    {
        Task<Response<IEnumerable<IncidentTypeDto>>> GetAllAsync();
        Task<Response<IncidentTypeDto>> CreateAsync(CreateIncidentTypeDto incidentTypeDto);
        Task<Response<bool>> UpdateAsync(int id, CreateIncidentTypeDto incidentTypeDto);
        Task<Response<bool>> SoftDeleteAsync(int id);
        Task<Response<List<IncidentTypeDto?>>> GetIncidentType(int incidentId);
    }
}
