namespace SafeReport.Web.DTOs
{
    public class PagedResultDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<ReportDTO> Reports { get; set; } = new List<ReportDTO>();

    }
}
