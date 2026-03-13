using System.Linq.Expressions;

namespace MyWebApi.Domain.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        //取得所有
        IQueryable<TEntity> GetAllAsync();
        //透過ID找單筆
        Task<TEntity> GetByIdAsync(object id);
        //條件單筆搜尋
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        //條件多筆搜尋
        Task <IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
        //檢查是否存在
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        //新增
        Task AddAsync(TEntity entity);
        //批次新增
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        //修改
        void Update(TEntity entity);
        //刪除(傳入實體)
        void Remove(TEntity entity);
        //刪除(傳入ID)
        Task RemoveByIdAsync(object id);
        //設定RowVersion
        void SetRowVersion(TEntity entity, byte[] rowVersion);
    }
}
