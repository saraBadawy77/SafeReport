using SafeReport.Application.Interfaces;
using SafeReport.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Core.Interfaces
{
	public interface IReportRepository:IBaseRepository<Report>
	{
		Task<int> GetTotalCountAsync();
        Task<int> GetTotalCountAsync(Expression<Func<Report, bool>>? predicate = null);
    }
}
