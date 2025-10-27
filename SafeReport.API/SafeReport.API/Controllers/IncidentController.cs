using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;

namespace SafeReport.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    public class IncidentController : ControllerBase
	{
		private readonly IIncidentService _incidentService;

		public IncidentController(IIncidentService incidentService)
		{
			_incidentService = incidentService;
		}

		[HttpGet]
		public async Task<ActionResult<Response<IEnumerable<IncidentDto>>>> GetAll()
		{
			var response = await _incidentService.GetAllAsync();
			return Ok(response);
		}

		[HttpPost]
		public async Task<ActionResult<Response<IncidentDto>>> Create([FromBody] CreateIncidentDto dto)
		{
			var response = await _incidentService.CreateAsync(dto);
			if (!response.Success)
				return BadRequest(response);

			return Ok(response);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<Response<bool>>> Update(int id, [FromBody] CreateIncidentDto dto)
		{
			var response = await _incidentService.UpdateAsync(id, dto);
			if (!response.Success)
				return NotFound(response);

			return Ok(response);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<Response<bool>>> Delete(int id)
		{
			var response = await _incidentService.SoftDeleteAsync(id);
			if (!response.Success)
				return NotFound(response);

			return Ok(response);
		}
	}
}
