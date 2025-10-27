using Microsoft.EntityFrameworkCore;
using SafeReport.Application.Interfaces;
using SafeReport.Core.Interfaces;
using SafeReport.Infrastructure.Context;
using System.Linq.Expressions;

namespace SafeReport.Infrastructure.Common
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly SafeReportDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(SafeReportDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void SoftDelete(T entity)
        {
            if (entity is ISoftDelete softDeletable)
            {
                softDeletable.IsDeleted = true;
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(
          int pageNumber,
          int pageSize,
          Expression<Func<T, bool>> predicate,
          params Expression<Func<T, object>>[]? includes)
        {
            IQueryable<T> query = _dbSet;

            // Apply soft delete filter if the entity supports it
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            // Apply custom filter if provided
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return await query.AsNoTracking().ToListAsync();
        }
        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[]? includes = null)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            // Apply includes dynamically
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return query.AsNoTracking().FirstOrDefault();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return await query.CountAsync();
        }





    }
}
