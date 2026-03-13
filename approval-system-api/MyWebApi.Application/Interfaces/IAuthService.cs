using MyWebApi.Application.DTOs.Request;

namespace MyWebApi.Application.Interfaces
{
    public interface IAuthService
    {
        public Task<string> GenerateStandardToken(Login_Req user);
        public UserDataBase GetUserData();
    }
}
