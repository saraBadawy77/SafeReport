using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Application.ISevices
{
	public interface IIncidentService
	{
		Task<Response<IEnumerable<IncidentDto>>> GetAllAsync();
		Task<Response<IncidentDto>> CreateAsync(CreateIncidentDto createIncidentDto);
		Task<Response<bool>> UpdateAsync(int id, CreateIncidentDto createIncidentDto);
		Task<Response<bool>> SoftDeleteAsync(int id);
		Task<Response<List<IncidentTypeDto?>>> GetIncidentType(int incidentId);
	}

}
