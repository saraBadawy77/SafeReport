using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Application.DTOs
{
	public class IncidentTypeDto
	{
		public int Id { get; set; }
		public string NameEn { get; set; }
		public string NameAr { get; set; }
		public string? DescriptionEn { get; set; }
		public string? DescriptionAr { get; set; }
		public DateTime Creationdate { get; set; }
	}
}
