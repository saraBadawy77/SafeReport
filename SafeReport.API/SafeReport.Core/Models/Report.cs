using SafeReport.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeReport.Core.Models
{
    [Table("Report")]
    public class Report : ISoftDelete
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int IncidentId { get; set; }
        public int IncidentTypeId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? ImagePath { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        public Incident Incident { get; set; } = null!;

    }
}
