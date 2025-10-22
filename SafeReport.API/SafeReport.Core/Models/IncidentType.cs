using SafeReport.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Core.Models
{
	[Table("IncidentType")]
	public class IncidentType : ISoftDelete
	{
		public int Id { get; set; }

		public string NameEn { get; set; }
		public string NameAr { get; set; }

		public string? DescriptionEn { get; set; }
		public string? DescriptionAr { get; set; }

		public DateTime CreationDate { get; set; } = DateTime.UtcNow;
		public bool IsDeleted { get; set; }

		public int IncidentId { get; set; }
		public virtual Incident Incident { get; set; } 
	}
}
