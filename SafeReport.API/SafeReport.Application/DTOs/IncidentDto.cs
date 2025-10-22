namespace SafeReport.Application.DTOs
{
    public class IncidentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime Creationdate { get; set; }
    }
}
