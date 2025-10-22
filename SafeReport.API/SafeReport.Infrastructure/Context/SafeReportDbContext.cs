using Microsoft.EntityFrameworkCore;
using SafeReport.Core.Models;

namespace SafeReport.Infrastructure.Context
{
    public class SafeReportDbContext(DbContextOptions<SafeReportDbContext> options) : DbContext(options)
    {
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentType> IncidentTypes { get; set; }
        public DbSet<Report> Reports { get; set; }
    }

}
