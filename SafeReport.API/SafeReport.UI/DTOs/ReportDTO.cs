namespace SafeReport.UI.DTOs;

public class ReportDTO
{
    public int Id { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
