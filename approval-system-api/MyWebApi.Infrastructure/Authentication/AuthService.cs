using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.Interfaces;
using MyWebApi.Domain.Constants;
using MyWebApi.Domain.Entities;
using MyWebApi.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyWebApi.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        private IHttpContextAccessor _httpContextAccessor; 
        private IConfiguration _configuration; 
        private IGenericRepository<SystemUser> _systemUserRepo;

        public AuthService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IGenericRepository<SystemUser> systemUserRepo)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _systemUserRepo = systemUserRepo;
        }

        //產生JWT Token
        public async Task<string> GenerateStandardToken(Login_Req userDto)
        {
            var user = await _systemUserRepo.FirstOrDefaultAsync(x => x.Id == userDto.Id);
            if (user == null)
            {
                throw new ArgumentException("帳號不存在");
            }

            //Payload
            var claims = new List<Claim>
            {
                new Claim(SystemIdentify.UserId, user.Id.ToString()), //UserID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //TokenID
                new Claim(SystemIdentify.Name, user.UserName),
                new Claim(SystemIdentify.Level, user.Level.ToString()), //使用者層級
                new Claim(SystemIdentify.Dept, user.DeptId)
            };

            var keyStr = _configuration[ConfigKeys.Jwt.Key];
            var issuer = _configuration[ConfigKeys.Jwt.Issuer];
            var audience = _configuration[ConfigKeys.Jwt.Audience];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1000),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public UserDataBase GetUserData()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userData = new UserDataBase
            {
                Id = int.Parse(user.FindFirstValue(SystemIdentify.UserId) ?? "0"),
                UserName = user.FindFirstValue(SystemIdentify.Name),
                Level = int.Parse(user.FindFirstValue(SystemIdentify.Level) ?? "0"),
                Dept = user.FindFirstValue(SystemIdentify.Dept)
            };

            return userData;
        }
    }
}
