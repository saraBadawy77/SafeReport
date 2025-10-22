using Microsoft.EntityFrameworkCore;
using SafeReport.Core.Interfaces;
using SafeReport.Core.Models;
using SafeReport.Infrastructure.Common;
using SafeReport.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Infrastructure.Repositories
{
	public class ReportRepository(SafeReportDbContext context) : BaseRepository<Report>(context), IReportRepository
	{
		public async Task<int> GetTotalCountAsync()
		{
			return await _dbSet.CountAsync();
		}

	}
}
