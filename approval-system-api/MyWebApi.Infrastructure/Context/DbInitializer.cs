using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Infrastructure.Context
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApprovalSystemContext>();

            //專案啟動時，自動建立資料表
            await context.Database.MigrateAsync();

            await context.Database.BeginTransactionAsync();

            //檢查資料是否已存在
            if (!context.SystemUser.Any())
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[SystemUser] ON");

                var users = new List<SystemUser>
                {
                    new SystemUser { Id = 1, UserName = "一般職員", Level = 1, CreateDate = DateTime.Now, SupervisorId = 2, DeptId = "L1" },
                    new SystemUser { Id = 2, UserName = "經理", Level = 1, CreateDate = DateTime.Now, SupervisorId = 3, DeptId = "L" },
                    new SystemUser { Id = 3, UserName = "董事長", Level = 3, CreateDate = DateTime.Now, SupervisorId = null, DeptId = "SSS" }
                };

                context.SystemUser.AddRange(users);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[SystemUser] OFF");
            }

            if (!context.ApplicationForm.Any())
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[ApplicationForm] ON");

                var forms = new List<ApplicationForm>
                {
                    new ApplicationForm { ApplicationNo = 1, ApplicationDate = DateTime.Now, ApplicantId = 1, Reason = "電腦零件採購申請", CreateDate = DateTime.Now, Status = 1 },
                    new ApplicationForm { ApplicationNo = 2, ApplicationDate = DateTime.Now, ApplicantId = 2, Reason = "電子產品與周邊商品請購", CreateDate = DateTime.Now, Status = 1 }
                };

                context.ApplicationForm.AddRange(forms);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[ApplicationForm] OFF");
            }

            if (!context.ApplicationFormDetail.Any())
            {
                var details = new List<ApplicationFormDetail>
                {
                    new ApplicationFormDetail { AppFormNo = 1, ItemName = "GeForce RTX™ 5090 WINDFORCE OC 32G", Unit = "片", Quantity = 1, Price = 120000, CreateDate = DateTime.Now },
                    new ApplicationFormDetail { AppFormNo = 1, ItemName = "AMD Ryzen9 9950X3D", Unit = "片", Quantity = 1, Price = 22999, CreateDate = DateTime.Now },
                    new ApplicationFormDetail { AppFormNo = 2, ItemName = "Nintendo Switch 2", Unit = "臺", Quantity = 5, Price = 9900, CreateDate = DateTime.Now },
                    new ApplicationFormDetail { AppFormNo = 2, ItemName = "薩爾達傳說 曠野之息", Unit = "片", Quantity = 3, Price = 2299, CreateDate = DateTime.Now },
                    new ApplicationFormDetail { AppFormNo = 2, ItemName = "瑪利歐賽車世界", Unit = "片", Quantity = 5, Price = 2999, CreateDate = DateTime.Now }
                };

                context.ApplicationFormDetail.AddRange(details);
                await context.SaveChangesAsync();
            }

            await context.Database.CommitTransactionAsync();
        }
    }
}
