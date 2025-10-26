using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.ISevices;

namespace SafeReport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public UserAuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Response<AuthResultDto>>> Register([FromBody] RegisterDto dto)
        {
            var result = await _identityService.RegisterAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response<AuthResultDto>>> Login([FromBody] LoginDto dto)
        {
            var result = await _identityService.LoginAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<Response<string>>> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = await _identityService.ForgotPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }




    }
}
