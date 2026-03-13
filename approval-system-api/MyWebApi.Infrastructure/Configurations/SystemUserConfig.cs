using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Infrastructure.Configurations
{
    public class SystemUserConfig : IEntityTypeConfiguration<SystemUser>
    {
        public void Configure(EntityTypeBuilder<SystemUser> builder)
        {
            //表名與主鍵
            builder.ToTable("SystemUser");
            builder.HasKey(t => t.Id);

            builder.HasQueryFilter(t => t.DeletedAt == null);

            //主鍵設定(Identity)
            builder.Property(t => t.Id)
                   .UseIdentityColumn();

            //欄位設定
            builder.Property(t => t.UserName)
                       .HasMaxLength(10)
                       .IsRequired(true);

            builder.Property(t => t.Level)
                       .HasMaxLength(10)
                       .IsRequired(true);

            builder.Property(t => t.CreateDate)
                       .HasDefaultValueSql("GETDATE()")
                       .IsRequired(true);

            builder.Property(t => t.DeletedAt)
                       .IsRequired(false);

            builder.Property(t => t.SupervisorId)
                       .IsRequired(false);

            builder.Property(t => t.DeptId)
                       .HasMaxLength(5)
                       .IsRequired(true);
        }
    }
}
