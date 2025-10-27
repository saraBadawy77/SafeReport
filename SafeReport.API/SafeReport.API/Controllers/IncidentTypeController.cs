using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;

namespace SafeReport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentTypeController : ControllerBase
    {
        private readonly IIncidentTypeService _incidentTypeService;

        public IncidentTypeController(IIncidentTypeService incidentTypeService)
        {
            _incidentTypeService = incidentTypeService;
        }

        /// <summary>
        /// Get all incident types.
        /// </summary>
        [HttpGet("GetAllTypes")]
        public async Task<ActionResult<Response<IEnumerable<IncidentTypeDto>>>> GetAll()
        {
            var response = await _incidentTypeService.GetAllAsync();
            return Ok(response);
        }

        /// <summary>
        /// Get incident types by incidentId.
        /// </summary>
        [HttpGet("GetByIncidentId{incidentId}")]
        public async Task<ActionResult<Response<List<IncidentTypeDto>>>> GetByIncidentId(int incidentId)
        {
            var response = await _incidentTypeService.GetIncidentType(incidentId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Create a new incident type.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Response<IncidentTypeDto>>> Create([FromBody] CreateIncidentTypeDto dto)
        {
            var response = await _incidentTypeService.CreateAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Update an existing incident type.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Response<bool>>> Update(int id, [FromBody] CreateIncidentTypeDto dto)
        {
            var response = await _incidentTypeService.UpdateAsync(id, dto);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Soft delete an incident type by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<bool>>> Delete(int id)
        {
            var response = await _incidentTypeService.SoftDeleteAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}
