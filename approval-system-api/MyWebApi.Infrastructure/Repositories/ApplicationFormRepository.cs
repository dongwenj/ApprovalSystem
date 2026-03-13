using Dapper;
using Microsoft.Extensions.Logging;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Application.Interfaces;
using MyWebApi.Domain.Entities;
using MyWebApi.Domain.Enums;
using MyWebApi.Domain.Interfaces;
using MyWebApi.Infrastructure.Context;
using System.Text;

namespace MyWebApi.Infrastructure.Repositories
{
    public class ApplicationFormRepository : GenericRepository<ApplicationForm>, IApplicationFormRepository
    {
        private readonly ISqlConnectionFactory _dbFactory;
        private readonly ILogger<ApplicationFormRepository> _logger;

        public ApplicationFormRepository(ApprovalSystemContext context, ISqlConnectionFactory dbFactory, ILogger<ApplicationFormRepository> logger) : base(context)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<ApplicationFormQuery_Res>> QueryAsync(ApplicationFormQuery_Req searchModel)
        {
            using var conn = _dbFactory.CreateConnection();
            StringBuilder sb = new StringBuilder();
            DynamicParameters parms = new DynamicParameters();

            sb.AppendLine(@"
SELECT 
af.ApplicationNo,
af.ApplicationDate,
su.UserName,
su.DeptId,
af.Reason,
af.RowVersion
FROM ApplicationForm af
LEFT JOIN SystemUser su ON af.ApplicantId = su.Id AND su.DeletedAt IS NULL
WHERE 1=1
AND af.DeletedAt IS NULL
");

            if (!string.IsNullOrWhiteSpace(searchModel.Reason))
            {
                sb.AppendLine(" AND af.Reason LIKE @Reason ");
                parms.Add("@Reason", $"%{searchModel.Reason}%");
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationDate))
            {
                sb.AppendLine(" AND af.ApplicationDate >= @ApplicationDate ");
                parms.Add("@ApplicationDate", searchModel.ApplicationDate);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.DeptId))
            {
                sb.AppendLine(" AND su.DeptId = @DeptId ");
                parms.Add("@DeptId", searchModel.DeptId);
            }

            if (searchModel.User.Level == (int)UserLevel.General) //一般職員只能看到自己的申請單
            {
                sb.AppendLine(" AND su.Id = @Id ");
                parms.Add("@Id", searchModel.User.Id);
            }

            sb.AppendLine(" ORDER BY af.ApplicationDate DESC "); //依申請日期排序(分頁用)
            sb.AppendLine(" OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY ");

            var skip = (searchModel.PageNumber - 1) * searchModel.PageSize;
            parms.Add("@Skip", skip);
            parms.Add("@PageSize", searchModel.PageSize);

            var result = await conn.QueryAsync<ApplicationFormQuery_Res>(sb.ToString(), parms);

            _logger.LogInformation("[{Method}] 查詢完成，回傳 {Count} 筆資料", nameof(QueryAsync), result.Count());
            return result;
        }

        public async Task<ApplicationFormView_Res> ViewAsync(ApplicationFormView_Req model)
        {
            using var conn = _dbFactory.CreateConnection();
            StringBuilder sb = new StringBuilder();
            DynamicParameters parms = new DynamicParameters();

            sb.AppendLine(@"
        SELECT 
        af.ApplicationNo,
        af.ApplicationDate,
        af.Reason,
        su.UserName,
        su.DeptId
    FROM ApplicationForm af
    LEFT JOIN SystemUser su ON af.ApplicantId = su.Id
    WHERE af.ApplicationNo = @ApplicationNo;

        SELECT Id,ItemName,Unit,Quantity,Price FROM ApplicationFormDetail WHERE AppFormNo = @ApplicationNo;
");

            using (var multi = await conn.QueryMultipleAsync(sb.ToString(), new { model.ApplicationNo }))
            {
                var master = await multi.ReadSingleOrDefaultAsync<ApplicationFormView_Res>();

                if (master != null)
                {
                    master.Items = (await multi.ReadAsync<ApplicationFormView_Res.Item>()).ToList();
                }

                _logger.LogInformation("[{Method}] 查詢完成", nameof(QueryAsync));
                return master;
            }
        }
    }
}
