using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using MyWebApi.Domain.Interfaces;
using MyWebApi.Infrastructure.Context;

namespace MyWebApi.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApprovalSystemContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApprovalSystemContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null) throw new InvalidOperationException("Transaction 已經開始");

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null) throw new InvalidOperationException("尚未開始 Transaction");

            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null) return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            var entries = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
            foreach (var entry in entries)
            {
                if (entry.Properties.Any(x => x.Metadata.Name == "UpdateDate"))
                {
                    entry.Property("UpdateDate").CurrentValue = DateTime.Now;
                }
            }

            return await _context.SaveChangesAsync();
        }
    }
}
