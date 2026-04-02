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
                    new SystemUser { Id = 1, UserName = "小王", Level = 1, CreateDate = DateTime.Now, SupervisorId = 2, DeptId = "L1" },
                    new SystemUser { Id = 2, UserName = "小明", Level = 2, CreateDate = DateTime.Now, SupervisorId = 3, DeptId = "L" },
                    new SystemUser { Id = 3, UserName = "小董", Level = 3, CreateDate = DateTime.Now, SupervisorId = null, DeptId = "SSS" }
                };

                context.SystemUser.AddRange(users);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[SystemUser] OFF");
            }

            if (!context.ApplicationForm.Any())
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[ApplicationForm] ON");

                var forms = new List<ApplicationForm>();
                var random = new Random();
                int totalCount = 200; // 你想要產生的總單數

                string[] productCategories = { "電腦零件", "辦公設備", "周邊耗材", "軟體授權" };

                for (int i = 1; i <= totalCount; i++)
                {
                    int status = 1;
                    int? signerId = null;
                    int? submitterId = null;
                    if (i > 150)
                    {
                        status = 2;
                        signerId = 3;
                        submitterId = 1;
                    }
                    else if(i > 100)
                    {
                        status = 2;
                        signerId = 2;
                        submitterId = 1;
                    }
                    else if (i > 80)
                    {
                        status = 4;
                    }
                    else
                    {
                        status = 1;
                    }

                    forms.Add(new ApplicationForm
                    {
                        ApplicationNo = i,
                        ApplicationDate = DateTime.Now.AddDays(-random.Next(1, 60)), // 隨機過去 60 天內
                        ApplicantId = 1,
                        Reason = $"{productCategories[random.Next(productCategories.Length)]}採購申請 - 序號 {i}",
                        CreateDate = DateTime.Now,
                        Status = status,
                        SignerId = signerId,
                        SubmitterId = submitterId,
                    });
                }

                context.ApplicationForm.AddRange(forms);
                await context.SaveChangesAsync();

                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[ApplicationForm] OFF");
            }

            if (!context.ApplicationFormDetail.Any())
            {
                var details = new List<ApplicationFormDetail>();
                var random = new Random();

                // 取得剛才建立的所有表單 ID
                var formIds = context.ApplicationForm.Select(f => f.ApplicationNo).ToList();

                string[] items = { "RTX 5090", "Ryzen 9950X3D", "DDR5 64GB RAM", "NVMe Gen5 SSD", "4K Monitor" };
                string[] units = { "片", "臺", "組", "個" };

                foreach (var formNo in formIds)
                {
                    // 每張單據隨機產生 1 到 5 筆明細
                    int detailCount = random.Next(1, 6);
                    for (int j = 0; j < detailCount; j++)
                    {
                        details.Add(new ApplicationFormDetail
                        {
                            AppFormNo = formNo, // 確保關聯到正確的主表
                            ItemName = $"{items[random.Next(items.Length)]} (批次-{formNo})",
                            Unit = units[random.Next(units.Length)],
                            Quantity = random.Next(1, 10),
                            Price = random.Next(1000, 150000),
                            CreateDate = DateTime.Now
                        });
                    }
                }

                context.ApplicationFormDetail.AddRange(details);
                await context.SaveChangesAsync();
            }

            await context.Database.CommitTransactionAsync();
        }
    }
}
