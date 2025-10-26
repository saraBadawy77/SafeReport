
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SafeReport.API.Extensions;
using SafeReport.Application.Helper;
using SafeReport.Application.ISevices;
using SafeReport.Application.Mappings;
using SafeReport.Application.Services;
using SafeReport.Infrastructure.Context;
using SafeReport.Infrastructure.Identity;
using System.Security.Principal;
using System.Text;

namespace SafeReport.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();

            builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));
            // Identity
            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddSignalR();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddHttpContextAccessor();
            builder.Host.UseSerilogConfiguration(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorClient", policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7252") 
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

			var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<RequestCultureMiddleware>();
            app.UseCors("AllowBlazorClient");
            app.UseHttpsRedirection();
            app.MapHub<ReportHub>("/reportHub");
            app.UseAuthorization();
            app.UseStaticFiles();
            app.MapControllers();

            app.Run();
        }
    }
}
