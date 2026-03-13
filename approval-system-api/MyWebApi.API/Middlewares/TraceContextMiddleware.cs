using MyWebApi.Application.Interfaces;
using MyWebApi.Domain.Constants;
using Serilog.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyWebApi.API.Middlewares
{
    public class TraceContextMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //取得本次請求的唯一ID
            string traceId = context.TraceIdentifier;

            var user = context.User;
            string userId = SystemIdentify.Anonymous;
            string userLevel = string.Empty;
            string userDept = string.Empty;
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                userId = user.FindFirst(SystemIdentify.UserId)?.Value ?? SystemIdentify.UnknownUser;
                userLevel = user.FindFirst(SystemIdentify.Level)?.Value ?? string.Empty;
                userDept = user.FindFirst(SystemIdentify.Dept)?.Value ?? string.Empty;
            }

            //推入LogContext,讓之後的LOG都自帶屬性
            using (LogContext.PushProperty(SystemIdentify.TraceId, traceId))
            {
                await _next(context);
            }
        }
    }
}
