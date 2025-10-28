using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Core.Models
{
    [Table("ReportImage")]
    public class ReportImage
    {
        public int Id { get; set; }
        public Guid ReportId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Report Report { get; set; } = null!;
    }
}
