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
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SafeReportDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new SafeReportDbContext(optionsBuilder.Options);
        }
    }
}
