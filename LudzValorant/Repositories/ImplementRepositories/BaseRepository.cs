using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.DataContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.Repositories.ImplementRepositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected IDbContext _IDbcontext = null;
        protected DbSet<TEntity> _dbset;
        protected DbContext _dbContext;
        protected DbSet<TEntity> DBSet
        {
            get
            {
                if (_dbset == null)
                {
                    _dbset = _dbContext.Set<TEntity>() as DbSet<TEntity>;

                }
                return _dbset;
            }
        }

        public BaseRepository(IDbContext dbContext)
        {
            _IDbcontext = dbContext;
            _dbContext = (DbContext)dbContext;
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query = expression != null ? DBSet.Where(expression) : DBSet;
            return await query.CountAsync();
        }

        public async Task<int> CountAsync(string include, Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query;
            if (!string.IsNullOrEmpty(include))
            {
                query = BuildQueryable(new List<string> { include }, expression);
                return await query.CountAsync();
            }
            return await CountAsync(expression);
        }
        protected IQueryable<TEntity> BuildQueryable(List<string> includes, Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query = DBSet.AsQueryable();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            if (includes != null && includes.Count > 0)
            {
                foreach (string include in includes)
                {
                    query.Include(include.Trim());

                }
            }
            return query;

        }
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            DBSet.Add(entity);
            await _IDbcontext.CommitChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities)
        {
            DBSet.AddRange(entities);
            await _IDbcontext.CommitChangesAsync();
            return entities;
        }

        public async Task DeleteAsync(Guid id)
        {
            var dataEntity = await DBSet.FindAsync(id);
            if (dataEntity != null)
            {
                DBSet.Remove(dataEntity);
                await _IDbcontext.CommitChangesAsync();
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
            IQueryable<TEntity> query = expression != null ? DBSet.Where(expression) : DBSet;
            var dataQuery = query;
            if (dataQuery != null)
            {
                DBSet.RemoveRange(dataQuery);
                await _IDbcontext.CommitChangesAsync();
            }
        }
        public async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                return;

            DBSet.RemoveRange(entities);
            await _IDbcontext.CommitChangesAsync();
        }
        public async Task<IQueryable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query = expression != null ? DBSet.Where(expression) : DBSet;
            return query;
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await DBSet.FirstOrDefaultAsync(expression);
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await DBSet.FindAsync(id);
        }
        public async Task<TEntity> GetByIdAsync(string id)
        {
            return await DBSet.FindAsync(id);
        }
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _IDbcontext.CommitChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            await _IDbcontext.CommitChangesAsync();
            return entities;
        }

        public async Task<(IEnumerable<TEntity> Items, int TotalItems)> GetPagedAsync(
            Expression<Func<TEntity, bool>> filter,
            int pageNumber,
            int pageSize)
        {
            var query = DBSet.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }
    }
}
