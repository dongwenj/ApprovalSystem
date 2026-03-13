using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Infrastructure.Configurations
{
    public class ApplicationFormConfig : IEntityTypeConfiguration<ApplicationForm>
    {
        public void Configure(EntityTypeBuilder<ApplicationForm> builder)
        {
            //表名與主鍵
            builder.ToTable("ApplicationForm");
            builder.HasKey(t => t.ApplicationNo);

            builder.HasQueryFilter(t => t.DeletedAt == null);

            //主鍵設定(Identity)
            builder.Property(t => t.ApplicationNo)
                   .UseIdentityColumn();

            //欄位設定
            builder.Property(t => t.ApplicationDate)
                   .IsRequired(true);

            builder.Property(t => t.ApplicantId)
                   .HasMaxLength(10)
                   .IsRequired(true);

            builder.Property(t => t.Reason)
                   .HasMaxLength(50)
                   .IsRequired(true);

            builder.Property(t => t.Status)
                   .IsRequired(true)
                   .HasDefaultValue(1);

            builder.Property(t => t.CreateDate)
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired(true);

            builder.Property(t => t.DeletedAt)
                   .IsRequired(false);

            builder.Property(t => t.SignerId)
                   .IsRequired(false);

            builder.Property(t => t.SubmitterId)
                   .IsRequired(false);

            builder.Property(t => t.UpdateDate)
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired(true);

            builder.Property(t => t.RowVersion)
                   .IsRowVersion();
        }
    }
}
