using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SafeReport.Infrastructure.Context
{
 
        public class SafeReportDbContextFactory : IDesignTimeDbContextFactory<SafeReportDbContext>
        {
            public SafeReportDbContext CreateDbContext(string[] args)
            {
                var basePath = Directory.GetCurrentDirectory();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                var connectionString = builder.GetConnectionString("DefaultConnection")
                    ?? "Server=.;Database=SafeReportDb;Trusted_Connection=True;MultipleActiveResultSets=true";

                var optionsBuilder = new DbContextOptionsBuilder<SafeReportDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                return new SafeReportDbContext(optionsBuilder.Options);
            }
        }
    
}
