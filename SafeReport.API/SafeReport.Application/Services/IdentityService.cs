using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafeReport.Application.Common;
using SafeReport.Application.DTOs;
using SafeReport.Application.Helper;
using SafeReport.Application.ISevices;
using SafeReport.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        public async Task<Response<AuthResultDto>> RegisterAsync(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return Response<AuthResultDto>.FailResponse("Passwords do not match");

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return Response<AuthResultDto>.FailResponse("Email already exists");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.Name
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return Response<AuthResultDto>.FailResponse(string.Join(",", createResult.Errors.Select(x => x.Description)));

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return Response<AuthResultDto>.SuccessResponse(
                new AuthResultDto { Email = user.Email, FullName = user.FullName },
                "Registration successful");
        }


        public async Task<Response<AuthResultDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Response<AuthResultDto>.FailResponse("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
                return Response<AuthResultDto>.FailResponse("Invalid credentials");

            var token = GenerateJwtToken(user);

            var result = new AuthResultDto
            {
                Token = token!,
                Email = user.Email,
                FullName = user.FullName
            };

            return Response<AuthResultDto>.SuccessResponse(result, "Login successful");
        }

        public async Task<Response<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Response<string>.FailResponse("User not found");

            string newPassword = GenerateStrongPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Response<string>.FailResponse(errors);
            }

            // Send the new password to the user’s email
            string body =
                $"Hello {user.FullName},<br><br>" +
                $"Your new password is: <strong>{newPassword}</strong><br>" +
                $"It is recommended to change it after login.<br><br>" +
                $"Thanks,<br>SafeReport Team 🚓🔥";

            await EmailSender.SendEmailAsync(dto.Email, "Your New Password", body);

            return Response<string>.SuccessResponse("A new password has been sent to your email");
        }

        /// <summary>
        /// Generates a secure password following rules: uppercase + digit + symbol + random letters.
        /// </summary>
        private string GenerateStrongPassword()
        {
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string digits = "0123456789";
            string symbols = "!@#$%^&*";
            string all = upper + digits + symbols;

            var rand = new Random();

            string password =
                $"{upper[rand.Next(upper.Length)]}" +
                $"{digits[rand.Next(digits.Length)]}" +
                $"{symbols[rand.Next(symbols.Length)]}" +
                $"{Guid.NewGuid().ToString("N")[..5]}"; // random mix makes length >= 8

            return password;
        }


        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwt = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
