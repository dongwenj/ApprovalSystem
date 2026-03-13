using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Application.Interfaces;
using MyWebApi.Application.Services;
using MyWebApi.Domain.Entities;
using MyWebApi.Domain.Enums;
using MyWebApi.Domain.Interfaces;
using System.Linq.Expressions;
using static MyWebApi.Application.DTOs.Request.ApplicationFormAdd_Req;

namespace MyWebApi.Tests.Service
{
    public class ApprovalServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IApplicationFormRepository> _mockAppFormRepo;
        private readonly Mock<IGenericRepository<ApplicationFormDetail>> _mockAppFormDetailRepo;
        private readonly Mock<IGenericRepository<SystemUser>> _mockSystemUserRepo;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IValidator<ApplicationFormAdd_Req>> _mockAddValidator;
        private readonly Mock<IValidator<ApplicationFormEdit_Req>> _mockUpdateValidator;
        private readonly Mock<ILogger<ApprovalService>> _mockLogger;
        private readonly Mock<IJobScheduler> _mockJobScheduler;
        private readonly Mock<IEmailService> _mockEmailService;

        private readonly ApprovalService _service;
        public ApprovalServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockAppFormRepo = new Mock<IApplicationFormRepository>();
            _mockAppFormDetailRepo = new Mock<IGenericRepository<ApplicationFormDetail>>();
            _mockSystemUserRepo = new Mock<IGenericRepository<SystemUser>>();
            _mockAuthService = new Mock<IAuthService>();
            _mockAddValidator = new Mock<IValidator<ApplicationFormAdd_Req>>();
            _mockUpdateValidator = new Mock<IValidator<ApplicationFormEdit_Req>>();
            _mockLogger = new Mock<ILogger<ApprovalService>>();
            _mockJobScheduler = new Mock<IJobScheduler>();
            _mockEmailService = new Mock<IEmailService>();

            _service = new ApprovalService(
            _mockUow.Object,
            _mockAppFormRepo.Object,
            _mockAppFormDetailRepo.Object,
            _mockSystemUserRepo.Object,
            _mockAuthService.Object,
            _mockAddValidator.Object,
            _mockUpdateValidator.Object,
            _mockLogger.Object,
            _mockJobScheduler.Object,
            _mockEmailService.Object);
        }

        #region ApplicationFormQuery
        [Fact]
        public async Task Query_ShouldReturnSuccessfully()
        {
            var user = new UserDataBase { Id = 1, Level = 1 };

            var fakeList = new List<ApplicationFormQuery_Res>
            {
                new ApplicationFormQuery_Res { ApplicationNo = "1" }
            };

            var searchModel = new ApplicationFormQuery_Req();

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.QueryAsync(It.IsAny<ApplicationFormQuery_Req>())).ReturnsAsync(fakeList);

            var result = await _service.ApplicationFormQuery(searchModel);

            Assert.NotNull(result);
            Assert.NotNull(searchModel.User);
            Assert.Equal(user.Id, searchModel.User.Id);

            _mockAuthService.Verify(x => x.GetUserData(), Times.Once);

            _mockAppFormRepo.Verify(x => x.QueryAsync(It.Is<ApplicationFormQuery_Req>(req => req.User == user)), Times.Once);
        }
        #endregion

        #region ApplicationFormView
        [Fact]
        public async Task GetById_ExistingId_ShouldReturnCorrectData()
        {
            var model = new ApplicationFormView_Req() { ApplicationNo = 1 };

            var viewData = new ApplicationFormView_Res()
            {
                ApplicationNo = 1
            };

            _mockAppFormRepo.Setup(x => x.ViewAsync(It.IsAny<ApplicationFormView_Req>())).ReturnsAsync(viewData);

            var result = await _service.ApplicationFormView(model);

            Assert.NotNull(result);
            Assert.Equal(1, result.ApplicationNo);

            _mockAppFormRepo.Verify(x => x.ViewAsync(It.Is<ApplicationFormView_Req>(req => req.ApplicationNo == model.ApplicationNo)), Times.Once);
        }
        #endregion

        #region ApplicationFormAdd
        [Fact]
        public async Task Add_ValidModel_ShouldSaveAndReturnId()
        {
            var user = new UserDataBase() { Id = 1 };
            var model = new ApplicationFormAdd_Req()
            {
                Items = new List<Item>()
                {
                    new Item(),
                    new Item(),
                }
            };

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAddValidator.Setup(x => x.ValidateAsync(It.IsAny<ApplicationFormAdd_Req>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _mockAppFormRepo.Setup(x => x.AddAsync(It.IsAny<ApplicationForm>())).Callback<ApplicationForm>(af => af.ApplicationNo = 1);

            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            _mockAppFormDetailRepo.Setup(x => x.AddAsync(It.IsAny<ApplicationFormDetail>()));

            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _service.ApplicationFormAdd(model);

            _mockAppFormRepo.Verify(x => x.AddAsync(It.Is<ApplicationForm>(af =>
            af.ApplicantId == 1 &&
            af.Status == (int)ApprovalStatus.Draft)), Times.Once);

            _mockAppFormDetailRepo.Verify(x => x.AddAsync(It.Is<ApplicationFormDetail>(afd => afd.AppFormNo == 1)), Times.Exactly(2));

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }
        #endregion

        #region ApplicationFormEdit
        [Fact]
        public async Task Edit_AllAdd_ShouldReturnCorrectCount()
        {
            int testAppNo = 1;

            //from client
            var model = new ApplicationFormEdit_Req() { ApplicationNo = testAppNo, Items = new List<ApplicationFormEdit_Req.Item>() 
            {
                new ApplicationFormEdit_Req.Item() { Id = 100, Price = 11, Quantity = 12 },
                new ApplicationFormEdit_Req.Item() { Id = 300, Price = 21, Quantity = 22 },
                new ApplicationFormEdit_Req.Item() { Id = 400, Price = 31, Quantity = 32 },
            }};

            //from database
            var afEntity = new ApplicationForm() { ApplicationNo = testAppNo };
            var appFormDetailEntityList = new List<ApplicationFormDetail>()
            {
                new ApplicationFormDetail() { AppFormNo = testAppNo, Id = 300 },
                new ApplicationFormDetail() { AppFormNo = testAppNo, Id = 400 },
                new ApplicationFormDetail() { AppFormNo = testAppNo, Id = 500 },
            };

            _mockUpdateValidator.Setup(x => x.ValidateAsync(It.IsAny<ApplicationFormEdit_Req>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(afEntity);

            _mockAppFormDetailRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ApplicationFormDetail, bool>>>())).ReturnsAsync(appFormDetailEntityList);

            _mockAppFormDetailRepo.Setup(x => x.AddAsync(It.IsAny<ApplicationFormDetail>()));

            _mockAppFormDetailRepo.Setup(x => x.Update(It.IsAny<ApplicationFormDetail>()));

            _mockAppFormDetailRepo.Setup(x => x.Remove(It.IsAny<ApplicationFormDetail>()));

            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.ApplicationFormEdit(model);

            _mockAppFormRepo.Verify(x => x.GetByIdAsync(model.ApplicationNo), Times.Once);

            _mockAppFormDetailRepo.Verify(x => x.AddAsync(It.Is<ApplicationFormDetail>(d => 
            d.Price == 11 &&
            d.Quantity == 12)), Times.Once);

            _mockAppFormDetailRepo.Verify(x => x.Update(It.Is<ApplicationFormDetail>(d => 
            d.AppFormNo == testAppNo &&
            d.Id == 300 &&
            d.Price == 21 &&
            d.Quantity == 22)), Times.Once);

            _mockAppFormDetailRepo.Verify(x => x.Update(It.Is<ApplicationFormDetail>(d =>
            d.AppFormNo == testAppNo &&
            d.Id == 400 &&
            d.Price == 31 &&
            d.Quantity == 32)), Times.Once);

            _mockAppFormDetailRepo.Verify(x => x.Remove(It.Is<ApplicationFormDetail>(d => d.Id == 500 && d.AppFormNo == testAppNo)), Times.Once);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }
        #endregion

        #region ApplicationFormDelete
        [Fact]
        public async Task Delete_ExistingId_ShouldRemoveFromDatabase()
        {
            var model = new ApplicationFormDelete_Req() { ApplicationNo = 1 };
            var entity = new ApplicationForm() { ApplicationNo = 1 };
            var detailEntity = new List<ApplicationFormDetail>()
            {
                new ApplicationFormDetail() { Id = 10 },
                new ApplicationFormDetail() { Id = 20 },
            };

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockAppFormDetailRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ApplicationFormDetail, bool>>>())).ReturnsAsync(detailEntity);

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

            _mockAppFormRepo.Setup(x => x.Remove(It.IsAny<ApplicationForm>()));

            _mockAppFormDetailRepo.Setup(x => x.Remove(It.IsAny<ApplicationFormDetail>()));

            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            var result = await _service.ApplicationFormDelete(model);

            _mockAppFormRepo.Verify(x => x.GetByIdAsync(model.ApplicationNo), Times.Once);
            _mockAppFormDetailRepo.Verify(x => x.WhereAsync(It.IsAny<Expression<Func<ApplicationFormDetail, bool>>>()), Times.AtLeastOnce);

            _mockAppFormRepo.Verify(x => x.Remove(It.Is<ApplicationForm>(af => af.ApplicationNo == 1)), Times.Once);
            _mockAppFormDetailRepo.Verify(x => x.Remove(It.IsAny<ApplicationFormDetail>()), Times.Exactly(2));

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_Exception_When_Data_Not_Found()
        {
            var model = new ApplicationFormDelete_Req() { ApplicationNo = 1 };

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ApplicationForm?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ApplicationFormDelete(model));

            Assert.Equal("找不到要刪除的資料", ex.Message);
        }
        #endregion

        #region ApplicationFormPresent
        [Fact]
        public async Task Present_OnDraft_ShouldReturnCorrectStatus()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormSubmit_Req() { ApplicationNo = testAppNo };
            var user = new UserDataBase() { Id = testUserId, Level = (int)UserLevel.General };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, SubmitterId = testUserId, Status = (int)ApprovalStatus.Draft };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = 20 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            _mockAppFormRepo.Setup(x => x.Update(It.IsAny<ApplicationForm>()));

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            await _service.ApplicationFormPresent(model);

            Assert.Equal((int)ApprovalStatus.Pending, entity.Status);
            Assert.Equal(systemUser.Id, entity.SubmitterId);
            Assert.Equal(systemUser.SupervisorId, entity.SignerId);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Present_OnRejected_ShouldReturnCorrectStatus()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormSubmit_Req() { ApplicationNo = testAppNo };
            var user = new UserDataBase() { Id = testUserId, Level = (int)UserLevel.General };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, SubmitterId = testUserId, Status = (int)ApprovalStatus.Rejected };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = 20 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            _mockAppFormRepo.Setup(x => x.Update(It.IsAny<ApplicationForm>()));

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            await _service.ApplicationFormPresent(model);

            Assert.Equal((int)ApprovalStatus.Pending, entity.Status);
            Assert.Equal(systemUser.Id, entity.SubmitterId);
            Assert.Equal(systemUser.SupervisorId, entity.SignerId);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Present_Exception_When_Data_Not_Found()
        {
            var model = new ApplicationFormSubmit_Req();
            var user = new UserDataBase();

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ApplicationForm?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ApplicationFormPresent(model));
            Assert.Equal("找不到要陳核的資料", exception.Message);
        }

        [Fact]
        public async Task Present_Exception_When_Wrong_Level()
        {
            var model = new ApplicationFormSubmit_Req();
            var user = new UserDataBase();

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationForm());

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ApplicationFormPresent(model));
            Assert.Equal("陳核人權限錯誤", exception.Message);
        }

        [Fact]
        public async Task Present_Exception_When_User_Not_Found()
        {
            var model = new ApplicationFormSubmit_Req();
            var user = new UserDataBase() { Level = (int)UserLevel.General };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationForm());
            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync((SystemUser?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ApplicationFormPresent(model));
            Assert.Equal("查無使用者資料", exception.Message);
        }

        [Fact]
        public async Task Present_Exception_When_Wrong_Status()
        {
            var model = new ApplicationFormSubmit_Req();
            var user = new UserDataBase() { Level = (int)UserLevel.General };
            var systemUser = new SystemUser();
            var entity = new ApplicationForm() { Status = (int)ApprovalStatus.Approved };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ApplicationFormPresent(model));
            Assert.Equal("只有狀態為「待陳核」或「退回」的申請單才能進行陳核", exception.Message);
        }
        #endregion

        #region ApplicationFormReview
        [Fact]
        public async Task Review_ApprovedByManager_ShouldReturnCorrectStatus()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormReview_Req() { ApplicationNo = testAppNo, IsApproved = true };
            var user = new UserDataBase() { Id = testUserId, Level = (int)UserLevel.Manager };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, ApplicantId = testUserId, Status = (int)ApprovalStatus.Pending };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = 20 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            _mockAppFormRepo.Setup(x => x.Update(It.IsAny<ApplicationForm>()));

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            await _service.ApplicationFormReview(model);

            _mockAppFormRepo.Verify(x => x.GetByIdAsync(testAppNo), Times.Once);
            _mockAppFormRepo.Verify(x => x.Update(It.Is<ApplicationForm>(af => af.Status == entity.Status && af.SignerId == systemUser.SupervisorId)), Times.Once);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Review_ApprovedByCEO_ShouldReturnCorrectStatus()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormReview_Req() { ApplicationNo = testAppNo, IsApproved = true };
            var user = new UserDataBase() { Id = testUserId, Level = (int)UserLevel.CEO };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, ApplicantId = testUserId, Status = (int)ApprovalStatus.Pending };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = null };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            _mockAppFormRepo.Setup(x => x.Update(It.IsAny<ApplicationForm>()));

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            await _service.ApplicationFormReview(model);

            Assert.Equal((int)ApprovalStatus.Approved, entity.Status);
            Assert.Equal(null, entity.SignerId);

            _mockAppFormRepo.Verify(x => x.GetByIdAsync(testAppNo), Times.Once);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Review_RejectedByManager_ShouldReturnCorrectStatus()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormReview_Req() { ApplicationNo = testAppNo, IsApproved = false };
            var user = new UserDataBase() { Id = testUserId, Level = (int)UserLevel.Manager };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, ApplicantId = testUserId, Status = (int)ApprovalStatus.Pending };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = 20 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            _mockAppFormRepo.Setup(x => x.Update(It.IsAny<ApplicationForm>()));

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            await _service.ApplicationFormReview(model);

            Assert.Equal((int)ApprovalStatus.Rejected, entity.Status);
            Assert.Equal(null, entity.SignerId);

            _mockAppFormRepo.Verify(x => x.GetByIdAsync(testAppNo), Times.Once);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Review_RejectedByCEO_ShouldReturnCorrectStatus()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormReview_Req() { ApplicationNo = testAppNo, IsApproved = false };
            var user = new UserDataBase() { Id = testUserId, Level = (int)UserLevel.CEO };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, ApplicantId = testUserId, Status = (int)ApprovalStatus.Pending };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = null };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);

            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);

            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            _mockAppFormRepo.Setup(x => x.Update(It.IsAny<ApplicationForm>()));

            _mockUow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockUow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            await _service.ApplicationFormReview(model);

            Assert.Equal((int)ApprovalStatus.Rejected, entity.Status);
            Assert.Equal(null, entity.SignerId);

            _mockAppFormRepo.Verify(x => x.GetByIdAsync(testAppNo), Times.Once);

            _mockUow.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUow.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockUow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Review_Exception_When_Unknown_Level()
        {
            int testUserId = 10;
            int testAppNo = 1;

            var model = new ApplicationFormReview_Req() { ApplicationNo = testAppNo, IsApproved = true };
            var user = new UserDataBase() { Id = testUserId, Level = 999 };
            var entity = new ApplicationForm() { ApplicationNo = testAppNo, ApplicantId = testUserId, Status = (int)ApprovalStatus.Pending };
            var systemUser = new SystemUser() { Id = testUserId, SupervisorId = 20 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
            _mockSystemUserRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<SystemUser, bool>>>())).ReturnsAsync(systemUser);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ApplicationFormReview(model));
            Assert.Equal("未知的簽核層級", exception.Message);
        }
        #endregion

        #region ApplicationFormSend
        [Fact]
        public async Task Send_ShouldSendEmail()
        {
            var user = new UserDataBase { Id = 1 };
            var model = new ApplicationFormSend_Req() { ApplicationNo = 10 };
            var entity = new ApplicationForm() { ApplicationNo = 10, ApplicantId = 1 };
            var detailEntity = new List<ApplicationFormDetail>()
            {
                new ApplicationFormDetail() { Id = 100, AppFormNo = 10 },
                new ApplicationFormDetail() { Id = 200, AppFormNo = 10 },
            };

            var systemUser = new SystemUser() { Id = 1 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
            _mockAppFormDetailRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ApplicationFormDetail, bool>>>())).ReturnsAsync(detailEntity);
            _mockSystemUserRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(systemUser);
            _mockJobScheduler.Setup(x => x.Schedule(It.IsAny<Expression<Action>>(), It.IsAny<TimeSpan>())).Returns(string.Empty);

            var result = await _service.ApplicationFormSend(model);

            _mockJobScheduler.Verify(x => x.Schedule(It.IsAny<Expression<Action>>(), TimeSpan.FromSeconds(5)), Times.Once());
        }

        [Fact]
        public async Task Send_Excetpion_When_Form_Not_Found()
        {
            var user = new UserDataBase { Id = 1 };
            var model = new ApplicationFormSend_Req() { ApplicationNo = 10 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ApplicationForm?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ApplicationFormSend(model));

            Assert.Equal("找不到申請單資料", exception.Message);
        }

        [Fact]
        public async Task Send_Excetpion_When_Detail_Not_Found()
        {
            var user = new UserDataBase { Id = 1 };
            var model = new ApplicationFormSend_Req() { ApplicationNo = 10 };
            var entity = new ApplicationForm() { ApplicationNo = 10, ApplicantId = 1 };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
            _mockAppFormDetailRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ApplicationFormDetail, bool>>>())).
                ReturnsAsync((List<ApplicationFormDetail>?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ApplicationFormSend(model));

            Assert.Equal("找不到申請單細項資料", exception.Message);
        }

        [Fact]
        public async Task Send_Excetpion_When_User_Not_Found()
        {
            var user = new UserDataBase { Id = 1 };
            var model = new ApplicationFormSend_Req() { ApplicationNo = 10 };
            var entity = new ApplicationForm() { ApplicationNo = 10, ApplicantId = 1 };
            var detailEntity = new List<ApplicationFormDetail>()
            {
                new ApplicationFormDetail() { Id = 100, AppFormNo = 10 },
                new ApplicationFormDetail() { Id = 200, AppFormNo = 10 },
            };

            _mockAuthService.Setup(x => x.GetUserData()).Returns(user);
            _mockAppFormRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(entity);
            _mockAppFormDetailRepo.Setup(x => x.WhereAsync(It.IsAny<Expression<Func<ApplicationFormDetail, bool>>>())).ReturnsAsync(detailEntity);
            _mockSystemUserRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SystemUser?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ApplicationFormSend(model));

            Assert.Equal("查無使用者資料", exception.Message);
        }
        #endregion
    }
}
