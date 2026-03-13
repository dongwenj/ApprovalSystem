using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Infrastructure.Configurations
{
    public class ApplicationFormDetailConfig : IEntityTypeConfiguration<ApplicationFormDetail>
    {
        public void Configure(EntityTypeBuilder<ApplicationFormDetail> builder)
        {
            //表名與主鍵
            builder.ToTable("ApplicationFormDetail");
            builder.HasKey(t => t.Id);

            builder.HasQueryFilter(t => t.DeletedAt == null);

            //主鍵設定(Identity)
            builder.Property(t => t.Id)
                   .UseIdentityColumn();

            //欄位設定
            builder.Property(t => t.AppFormNo)
                   .IsRequired(true);

            builder.Property(t => t.ItemName)
                   .HasMaxLength(100)
                   .IsRequired(true);

            builder.Property(t => t.Unit)
                    .HasMaxLength(5)
                   .IsRequired(true);

            builder.Property(t => t.Quantity)
                   .IsRequired(true);

            builder.Property(t => t.Price)
                   .IsRequired(true);

            builder.Property(t => t.CreateDate)
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired(true);

            builder.Property(t => t.DeletedAt)
                   .IsRequired(false);
        }
    }
}
