﻿namespace SafeReport.Application.DTOs
{
    public class ReportFilterDto
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 5;
        public int? IncidentId { get; set; }
        public int? IncidentTypeId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
