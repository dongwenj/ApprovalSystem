using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace MyWebApi.Infrastructure.Services
{
    public class LogHub : Hub
    {
    }

    public class SystemLogService : ILogEventSink
    {
        private readonly IServiceProvider _serviceProvider;

        public SystemLogService(IServiceProvider serviceProvider)
        { 
            _serviceProvider = serviceProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            // 直接從服務容器抓出剛才定義的 LogHub
            var hubContext = _serviceProvider.GetService<IHubContext<LogHub>>();

            if (hubContext != null)
            {
                string shortLevel = logEvent.Level switch
                {
                    LogEventLevel.Verbose => "VRB",
                    LogEventLevel.Debug => "DBG",
                    LogEventLevel.Information => "INF",
                    LogEventLevel.Warning => "WRN",
                    LogEventLevel.Error => "ERR",
                    LogEventLevel.Fatal => "FTL",
                    _ => logEvent.Level.ToString().Substring(0, 3).ToUpper()
                };

                // 組合你要顯示在網頁上的文字
                var message = $"[{logEvent.Timestamp:HH:mm:ss}] [{shortLevel}] {logEvent.RenderMessage()}";

                // 確保發送 Log 不會卡住 API 正常運作
                Task.Run(() => hubContext.Clients.All.SendAsync("ReceiveLog", message));
            }
        }
    }
}
