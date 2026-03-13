using Microsoft.AspNetCore.Mvc.Filters;
using MyWebApi.Domain.Constants;

namespace MyWebApi.API.Filters
{
    public class LogActionFilter : IActionFilter
    {
        private readonly ILogger<LogActionFilter> _logger;

        public LogActionFilter(ILogger<LogActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            #region 寫入開頭LOG
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            //取得使用者資訊
            string userId = SystemIdentify.Anonymous;
            string userLevel = string.Empty;
            string userDept = string.Empty;

            if (user?.Identity?.IsAuthenticated == true)
            {
                userId = user.FindFirst(SystemIdentify.UserId)?.Value ?? SystemIdentify.UnknownUser;
                userLevel = user.FindFirst(SystemIdentify.Level)?.Value ?? string.Empty;
                userDept = user.FindFirst(SystemIdentify.Dept)?.Value ?? string.Empty;
            }

            //取得Action和Controller的名稱
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            var actionName = context.ActionDescriptor.RouteValues["action"];

            //抓取DTO參數並排除掉一些系統內建的參數
            var args = context.ActionArguments
                .Where(x => !x.Key.Contains("context") && !x.Key.Contains("cancellation"))
                .ToDictionary(k => k.Key, v => v.Value);

            _logger.LogInformation(
                "[操作軌跡] 使用者:{UserId} (部門:{Dept}/權限:{Level}) 執行功能:{Controller}/{Action} 參數:{@Args}",
                userId,
                userDept,
                userLevel,
                controllerName,
                actionName,
                args
            ); 
            #endregion
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
