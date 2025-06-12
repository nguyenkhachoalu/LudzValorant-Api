using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Repositories.InterfaceRepositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<IQueryable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> expression = null);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression);
        Task<TEntity> GetByIdAsync(Guid id);
        Task<TEntity> GetByIdAsync(string id);
        Task<TEntity> CreateAsync(TEntity entity);
        Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(Expression<Func<TEntity, bool>> expression);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null);
        Task<int> CountAsync(string include, Expression<Func<TEntity, bool>> expression = null);
        Task<(IEnumerable<TEntity> Items, int TotalItems)> GetPagedAsync(Expression<Func<TEntity, bool>> filter,int pageNumber,int pageSize);
    }
}
