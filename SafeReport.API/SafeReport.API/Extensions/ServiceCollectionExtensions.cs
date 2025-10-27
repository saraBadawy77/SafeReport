using Microsoft.EntityFrameworkCore;
using SafeReport.Application.ISevices;
using SafeReport.Application.Services;
using SafeReport.Core.Interfaces;
using SafeReport.Infrastructure.Context;
using SafeReport.Infrastructure.Repositories;

namespace SafeReport.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SafeReportDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<IIncidentTypeRepository, IncidentTypeRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            return services;
        }



        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IIncidentService, IncidentService>();
            services.AddScoped<IIncidentTypeService, IncidentTypeService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IIdentityService, IdentityService>();
            return services;
        }
    }
}
