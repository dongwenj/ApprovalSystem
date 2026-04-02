using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Domain.Interfaces;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Application.Interfaces
{
    public interface IApplicationFormRepository : IGenericRepository<ApplicationForm>
    {
        Task<ApplicationFormQuery_Res> QueryAsync(ApplicationFormQuery_Req searchModel);
        Task<ApplicationFormView_Res> ViewAsync(ApplicationFormView_Req model);
    }
}
