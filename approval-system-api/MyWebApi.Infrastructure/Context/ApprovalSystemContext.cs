using Microsoft.EntityFrameworkCore;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Infrastructure.Context
{
    public class ApprovalSystemContext : DbContext
    {
        public ApprovalSystemContext(DbContextOptions<ApprovalSystemContext> options) : base(options)
        {
        }

        public DbSet<ApplicationForm> ApplicationForm => Set<ApplicationForm>();
        public DbSet<ApplicationFormDetail> ApplicationFormDetail => Set<ApplicationFormDetail>();
        public DbSet<SystemUser> SystemUser => Set<SystemUser>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApprovalSystemContext).Assembly);
        }
    }
}
