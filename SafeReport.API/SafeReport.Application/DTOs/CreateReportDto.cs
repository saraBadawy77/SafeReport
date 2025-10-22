﻿using Microsoft.AspNetCore.Http;

namespace SafeReport.Application.DTOs
{
    public class CreateReportDto
    {
        public string? Description { get; set; } = string.Empty;
        public int IncidentId { get; set; }
        public int IncidentTypeId { get; set; }
        public IFormFile? Image { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
