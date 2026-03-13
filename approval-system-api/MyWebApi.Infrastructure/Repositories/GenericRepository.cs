using Microsoft.EntityFrameworkCore;
using MyWebApi.Domain.Interfaces;
using MyWebApi.Infrastructure.Context;
using System.Linq.Expressions;
using static Dapper.SqlMapper;

namespace MyWebApi.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly ApprovalSystemContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApprovalSystemContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAllAsync()
        {
            return _dbSet;
        }

        public async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task RemoveByIdAsync(object id)
        {
            TEntity entityToDelete = await _dbSet.FindAsync(id);

            if (entityToDelete != null)
            {
                await RemoveByIdAsync(entityToDelete);
            }
        }

        public void SetRowVersion(TEntity entity, byte[] rowVersion)
        {
            _context.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;
        }
    }
}
