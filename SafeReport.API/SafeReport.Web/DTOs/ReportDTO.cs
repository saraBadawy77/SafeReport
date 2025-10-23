namespace SafeReport.Web.DTOs;

public class ReportDTO
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int IncidentId { get; set; }
    public string IncidentName { get; set; }

    public int IncidentTypeId { get; set; }
    public string IncidentTypeName { get; set; }
    public string Address { get; set; }
    public string Image { get; set; }
    public string? PhoneNumber { get; set; }
    public string TimeSinceCreated { get; set; } = string.Empty;
}
