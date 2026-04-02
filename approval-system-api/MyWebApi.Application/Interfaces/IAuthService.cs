using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Domain.Entities;

namespace MyWebApi.Application.Interfaces
{
    public interface IAuthService
    {
        public Task<string> GenerateStandardToken(SystemUser user);
        public Task<Login_Res> LoginInfo(Login_Req userDto);
        public UserDataBase GetUserData();
    }
}
