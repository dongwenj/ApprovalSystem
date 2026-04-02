using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Application.Interfaces;
using MyWebApi.Domain.Constants;
using MyWebApi.Domain.Entities;
using MyWebApi.Domain.Enums;
using MyWebApi.Domain.Interfaces;
using System.Text;

namespace MyWebApi.Application.Services;

public class ApprovalService
{
    private readonly IUnitOfWork _uow;
    private readonly IApplicationFormRepository _appFormRepo;
    private readonly IGenericRepository<ApplicationFormDetail> _appFormDetailRepo;
    private readonly IGenericRepository<SystemUser> _systemUserRepo;
    private readonly IAuthService _authService;
    private readonly IValidator<ApplicationFormAdd_Req> _addValidator;
    private readonly IValidator<ApplicationFormEdit_Req> _updateValidator;
    private readonly ILogger<ApprovalService> _logger;
    private readonly IJobScheduler _jobScheduler;
    private readonly IEmailService _emailService;

    public ApprovalService(
        IUnitOfWork uow,
        IApplicationFormRepository appFormRepo,
        IGenericRepository<ApplicationFormDetail> appFormDetailRepo,
        IGenericRepository<SystemUser> systemUserRepo,
        IAuthService authService,
        IValidator<ApplicationFormAdd_Req> addValidator,
        IValidator<ApplicationFormEdit_Req> updateValidator,
        ILogger<ApprovalService> logger,
        IJobScheduler jobScheduler,
        IEmailService emailService
        )
    {
        _uow = uow;
        _appFormRepo = appFormRepo;
        _appFormDetailRepo = appFormDetailRepo;
        _systemUserRepo = systemUserRepo;
        _authService = authService;
        _addValidator = addValidator;
        _updateValidator = updateValidator;
        _logger = logger;
        _jobScheduler = jobScheduler;
        _emailService = emailService;
    }

    //申請單列表查詢
    public async Task<ApplicationFormQuery_Res> ApplicationFormQuery(ApplicationFormQuery_Req searchModel)
    {
        var user = _authService.GetUserData();
        searchModel.User = user;

        var result = await _appFormRepo.QueryAsync(searchModel);

        return result;
    }

    //申請單查詢
    public async Task<ApplicationFormView_Res> ApplicationFormView(ApplicationFormView_Req model)
    {
        var result = await _appFormRepo.ViewAsync(model);
        return result;
    }

    //申請單新增
    public async Task<ApplicationFormAdd_Res> ApplicationFormAdd(ApplicationFormAdd_Req model)
    {
        ApplicationFormAdd_Res result = new ApplicationFormAdd_Res();
        var user = _authService.GetUserData();

        //驗整資料正確性
        await _addValidator.ValidateAndThrowAsync(model);

        await _uow.BeginTransactionAsync();

        ApplicationForm afAddModel = new ApplicationForm();
        afAddModel.ApplicationDate = model.ApplicationDate;
        afAddModel.Reason = model.Reason;
        afAddModel.ApplicantId = user.Id;
        afAddModel.Status = (int)ApprovalStatus.Draft; //待陳核

        await _appFormRepo.AddAsync(afAddModel);
        await _uow.SaveChangesAsync();

        for (int i = 0; i < model.Items.Count; i++)
        {
            await _appFormDetailRepo.AddAsync(new ApplicationFormDetail
            {
                AppFormNo = afAddModel.ApplicationNo,
                ItemName = model.Items[i].ItemName,
                Unit = model.Items[i].Unit,
                Quantity = model.Items[i].Quantity,
                Price = model.Items[i].Price,
            });
        }

        await _uow.SaveChangesAsync();
        await _uow.CommitAsync();

        _logger.LogInformation("[{Method}] {Table}新增完成, 單號:{AppNo}, 細項共{ItemCount}筆", nameof(ApplicationFormAdd), nameof(ApplicationForm), afAddModel.ApplicationNo, model.Items.Count);

        return result;
    }

    //申請單修改
    public async Task<ApplicationFormEdit_Res> ApplicationFormEdit(ApplicationFormEdit_Req model)
    {
        ApplicationFormEdit_Res result = new ApplicationFormEdit_Res();
        int itemCountAdd = 0;
        int itemCountUpdate = 0;
        int itemCountDelete = 0;

        #region 檢核
        //驗整資料正確性
        await _updateValidator.ValidateAndThrowAsync(model);

        var afEntity = await _appFormRepo.GetByIdAsync(model.ApplicationNo);
        if (afEntity == null)
        {
            throw new Exception("找不到要修改的資料");
        }
        #endregion

        //用主表ApplicationNo取得對應的明細
        var appFormDetailEntityList = await _appFormDetailRepo.WhereAsync(x => x.AppFormNo == model.ApplicationNo);

        _appFormRepo.SetRowVersion(afEntity, model.RowVersion);

        await _uow.BeginTransactionAsync();

        afEntity.ApplicationDate = model.ApplicationDate;
        afEntity.Reason = model.Reason;

        for (int i = 0; i < model.Items.Count; i++)
        {
            var entity2 = appFormDetailEntityList.FirstOrDefault(x => x.Id == model.Items[i].Id);
            if (entity2 == null) //資料庫無資料 = 新增
            {
                await _appFormDetailRepo.AddAsync(new ApplicationFormDetail
                {
                    AppFormNo = afEntity.ApplicationNo,
                    ItemName = model.Items[i].ItemName,
                    Unit = model.Items[i].Unit,
                    Quantity = model.Items[i].Quantity,
                    Price = model.Items[i].Price,
                });

                itemCountAdd++;
                continue;
            }
            else //資料庫有資料 = 修改
            {
                entity2.ItemName = model.Items[i].ItemName;
                entity2.Quantity = model.Items[i].Quantity;
                entity2.Price = model.Items[i].Price;
                entity2.Unit = model.Items[i].Unit;
                _appFormDetailRepo.Update(entity2);
                itemCountUpdate++;
            }
        }

        foreach (var entity2 in appFormDetailEntityList) //刪除資料庫有但前端無的資料
        {
            var deleteItem = model.Items.FirstOrDefault(x => x.Id == entity2.Id);
            if (deleteItem == null)
            {
                _appFormDetailRepo.Remove(entity2);
                itemCountDelete++;
            }
        }

        await _uow.SaveChangesAsync();
        await _uow.CommitAsync();

        _logger.LogInformation("[{Method}] {Table}修改完成, 單號:{AppNo}, 細項共{ItemCount}筆:新增{Add}, 修改{Upd}, 刪除{Del}",
            nameof(ApplicationFormEdit),
            nameof(ApplicationForm),
            model.ApplicationNo, 
            model.Items.Count, 
            itemCountAdd, 
            itemCountUpdate, 
            itemCountDelete);

        return result;
    }

    //申請單刪除
    public async Task<ApplicationFormDelete_Res> ApplicationFormDelete(ApplicationFormDelete_Req model)
    {
        ApplicationFormDelete_Res result = new ApplicationFormDelete_Res();

        #region 檢核
        var entity = await _appFormRepo.GetByIdAsync(model.ApplicationNo);
        if (entity == null)
        {
            throw new KeyNotFoundException("找不到要刪除的資料");
        }
        #endregion

        var appFormDetailEntityList = await _appFormDetailRepo.WhereAsync(x => x.AppFormNo == model.ApplicationNo);

        await _uow.BeginTransactionAsync();

        foreach (var item in appFormDetailEntityList)
        {
            _appFormDetailRepo.Remove(item);
        }

        _appFormRepo.Remove(entity);

        await _uow.SaveChangesAsync();
        await _uow.CommitAsync();

        _logger.LogInformation("[{Method}] {Table}刪除完成, 單號:{AppNo}, 細項共{ItemCount}筆", 
            nameof(ApplicationFormDelete), 
            nameof(ApplicationForm), 
            model.ApplicationNo,
            appFormDetailEntityList.Count());

        return result;
    }

    //申請單陳核
    public async Task<ApplicationFormSubmit_Res> ApplicationFormPresent(ApplicationFormSubmit_Req model)
    {
        ApplicationFormSubmit_Res result = new ApplicationFormSubmit_Res();
        var user = _authService.GetUserData();

        #region 檢核
        var entity = await _appFormRepo.GetByIdAsync(model.ApplicationNo);
        if (entity == null)
        {
            throw new KeyNotFoundException("找不到要陳核的資料");
        }

        if(user.Level != (int)UserLevel.General && user.Level != (int)UserLevel.Manager)
        {
            throw new ArgumentException("陳核人權限錯誤");
        }

        var systemUser = await _systemUserRepo.FirstOrDefaultAsync(x => x.Id == user.Id);
        if (systemUser == null)
        {
            throw new KeyNotFoundException("查無使用者資料");
        }

        if (entity.Status != (int)ApprovalStatus.Draft && entity.Status != (int)ApprovalStatus.Rejected)
        {
            throw new ArgumentException("只有狀態為「待陳核」或「退回」的申請單才能進行陳核");
        }
        #endregion

        await _uow.BeginTransactionAsync();

        entity.Status = (int)ApprovalStatus.Pending;
        entity.SubmitterId = user.Id;
        entity.SignerId = systemUser.SupervisorId;
        _appFormRepo.Update(entity);

        await _uow.SaveChangesAsync();
        await _uow.CommitAsync();

        _logger.LogInformation("[{Method}] {Table}陳核完成, 單號:{AppNo}, 陳核給{SignerId}", nameof(ApplicationFormPresent), nameof(ApplicationForm), model.ApplicationNo, systemUser.SupervisorId);

        return result;
    }
    
    //申請單簽核
    public async Task<ApplicationFormReview_Res> ApplicationFormReview(ApplicationFormReview_Req model)
    {
        ApplicationFormReview_Res result = new ApplicationFormReview_Res();
        var user = _authService.GetUserData();

        #region 檢核
        var entity = await _appFormRepo.GetByIdAsync(model.ApplicationNo);
        if (entity == null)
        {
            throw new KeyNotFoundException("找不到要簽核的資料");
        }

        var systemUser = await _systemUserRepo.FirstOrDefaultAsync(x => x.Id == user.Id);
        if (systemUser == null)
        {
            throw new KeyNotFoundException("查無使用者資料");
        }

        if (entity.Status != (int)ApprovalStatus.Pending)
        {
            throw new ArgumentException("只有狀態為「已陳核」的申請單才能進行簽核");
        }
        #endregion
        
        ApprovalStatus status = ApprovalStatus.Pending;
        int? nextSignerId = null; //下一位簽核人

        if (model.IsApproved)
        {
            if(user.Level == (int)UserLevel.Manager)
            {
                status = ApprovalStatus.Pending; 
                nextSignerId = systemUser.SupervisorId; //下一位簽核人

            }
            else if(user.Level == (int)UserLevel.CEO)
            {
                status = ApprovalStatus.Approved; //核准
                nextSignerId = null; //流程結束, 無下一位簽核人
            }
            else throw new ArgumentException("未知的簽核層級");
        }
        else
        {
            status = ApprovalStatus.Rejected; //退回
            nextSignerId = null;
        }

        await _uow.BeginTransactionAsync();

        entity.Status = (int)status;
        entity.SignerId = nextSignerId;
        _appFormRepo.Update(entity);

        await _uow.SaveChangesAsync();
        await _uow.CommitAsync();

        _logger.LogInformation("[{Method}] {Table}簽核完成, 單號:{AppNo}, 動作{Action}, 操作人{Operator}, 下一簽核人{NextSignerId}", 
            nameof(ApplicationFormReview), 
            nameof(ApplicationForm), 
            model.ApplicationNo,
            model.IsApproved ? "同意" : "退回",
            user.Id,
            nextSignerId);

        return result;
    }

    //寄送MAIL
    public async Task<ApplicationFormSend_Res> ApplicationFormSend(ApplicationFormSend_Req model)
    {
        ApplicationFormSend_Res result = new ApplicationFormSend_Res();
        var user = _authService.GetUserData();

        #region 檢核
        var entity = await _appFormRepo.GetByIdAsync(model.ApplicationNo);
        if (entity == null)
        {
            throw new KeyNotFoundException("找不到申請單資料");
        }

        var detailEntity = await _appFormDetailRepo.WhereAsync(x => x.AppFormNo == model.ApplicationNo);
        if (detailEntity == null)
        {
            throw new KeyNotFoundException("找不到申請單細項資料");
        }

        var systemUser = await _systemUserRepo.GetByIdAsync(user.Id);
        if (systemUser == null)
        {
            throw new KeyNotFoundException("查無使用者資料");
        }
        #endregion

        string subject = $"簽核系統通知信件";
        string toEmail = model.ToEmail;

        // 郵件內容
        var sb = new StringBuilder();

        //申請單主表
        sb.AppendLine($"<p>親愛的 {systemUser.UserName} 您好:</p>");
        sb.AppendLine("<p>以下是申請單內容：</p>");
        sb.AppendLine("<ul>");
        sb.AppendLine($"    <li><strong>單號：</strong> {entity.ApplicationNo}</li>");
        sb.AppendLine($"    <li><strong>申請日期：</strong> {entity.ApplicationDate:yyyy/MM/dd}</li>");
        sb.AppendLine($"    <li><strong>申請理由：</strong> {entity.Reason}</li>");
        sb.AppendLine("</ul>");

        //申請細項
        sb.AppendLine("<h3>申請細項</h3>");
        sb.AppendLine("<table border='1' cellpadding='5' style='border-collapse: collapse; width: 100%;'>");
        sb.AppendLine("    <thead>");
        sb.AppendLine("        <tr style='background-color: #f2f2f2;'>");
        sb.AppendLine("            <th>物品名稱</th>");
        sb.AppendLine("            <th>數量</th>");
        sb.AppendLine("            <th>單位</th>");
        sb.AppendLine("            <th>單價</th>");
        sb.AppendLine("            <th>小計</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </thead>");
        sb.AppendLine("    <tbody>");

        foreach (var item in detailEntity)
        {
            int subtotal = item.Quantity * item.Price;
            sb.AppendLine("        <tr>");
            sb.AppendLine($"            <td>{item.ItemName}</td>");
            sb.AppendLine($"            <td align='right'>{item.Quantity}</td>");
            sb.AppendLine($"            <td>{item.Unit}</td>");
            sb.AppendLine($"            <td align='right'>{item.Price:N0}</td>"); //N0格式化千分位
            sb.AppendLine($"            <td align='right'>{subtotal:N0}</td>");
            sb.AppendLine("        </tr>");
        }

        sb.AppendLine("    </tbody>");
        sb.AppendLine("</table>");

        sb.AppendLine("<br/><p>※ 此信件由系統自動發送，請勿直接回覆。</p>");

        _jobScheduler.Enqueue(() => _emailService.SendAsync(toEmail, subject, sb.ToString()));
        //_jobScheduler.Schedule(() => _emailService.SendAsync(toEmail, subject, sb.ToString()), TimeSpan.FromSeconds(5));

        return result;
    }
}
