using SafeReport.Application.Common;
using SafeReport.Application.DTOs;


namespace SafeReport.Application.ISevices
{
    public interface IIdentityService
    {
        Task<Response<AuthResultDto>> RegisterAsync(RegisterDto dto);
        Task<Response<AuthResultDto>> LoginAsync(LoginDto dto);
        Task<Response<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
    }
}
