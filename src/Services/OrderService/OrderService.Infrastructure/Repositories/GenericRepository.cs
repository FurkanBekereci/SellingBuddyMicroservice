using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces.Repositories;
using OrderService.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity> where TEntity : BaseEntity where TContext : DbContext, IUnitOfWork
    {
        private readonly TContext _dbContext;

        public GenericRepository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IUnitOfWork UnitOfWork => _dbContext;

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
            return entity;
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await _dbContext.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                query = query.Include(include);
            }

            if(filter != null)
            {
                query = query.Where(filter);
            }

            if(orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public virtual Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null, params Expression<Func<TEntity, object>>[] includes)
        {
            return GetAsync(filter, null, includes);
        }

        public virtual async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<TEntity> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> queryable = _dbContext.Set<TEntity>();

            foreach(Expression<Func<TEntity, object>> include in includes)
            {
                queryable = queryable.Include(include);
            }

            return await queryable.FirstOrDefaultAsync(q => q.Id == id);
        }

        public virtual async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> expression = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> queryable = _dbContext.Set<TEntity>();

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                queryable = queryable.Include(include);
            }

            return await queryable.Where(expression).FirstOrDefaultAsync();
        }

        public virtual TEntity Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
            return entity;
        }
    }
}
