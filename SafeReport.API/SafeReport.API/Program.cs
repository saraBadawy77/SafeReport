
using Microsoft.EntityFrameworkCore;
using SafeReport.API.Extensions;
using SafeReport.Application.Helper;
using SafeReport.Application.Mappings;

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
            builder.Services.AddApplicationServices();
            builder.Services.AddSignalR();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
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


            app.MapControllers();

            app.Run();
        }
    }
}
