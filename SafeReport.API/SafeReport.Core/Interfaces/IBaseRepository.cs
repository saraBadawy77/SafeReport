using System.Linq.Expressions;

namespace SafeReport.Application.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void SoftDelete(T entity);
        Task SaveChangesAsync();
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[]? includes);

        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[]? includes = null);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
