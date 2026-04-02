using Hangfire;
using Microsoft.OpenApi.Models;
using MyWebApi.API.Filters;
using MyWebApi.API.Middlewares;
using MyWebApi.Application;
using MyWebApi.Domain.Constants;
using MyWebApi.Infrastructure;
using MyWebApi.Infrastructure.Context;
using MyWebApi.Infrastructure.Services;
using Serilog;
using System.Text.Encodings.Web;
using System.Text.Unicode;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //DI
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        //Serilog設定
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Sink(new SignalRSink(services)) //加入即時日誌轉發插件
            //排除 SignalR 內部的日誌，避免連線動作產生過多雜訊
            .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Http.Connections", Serilog.Events.LogEventLevel.Warning));

        // 建立 CORS 規則
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MyReactAppPolicy", policy =>
            {
                policy.WithOrigins(
                    "http://localhost:5173",   // CRA 本地開發 (npm run dev)
                    "http://localhost",        // Docker 內部的 Nginx (80 Port)
                    "http://approvalsystem.sytes.net", // AWS EC2
                    "https://approvalsystem.sytes.net" // AWS EC2
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<LogActionFilter>(); //註冊全域Filter(Controller的LOG自動化)
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.CustomSchemaIds(type =>
            {
                //使用完整命名空間 + 類別名稱，避免重名
                return type.FullName?.Replace("+", ".");
            });

            //加入JWT驗證設定
            c.AddSecurityDefinition(SystemIdentify.Bearer, new OpenApiSecurityScheme
            {
                Name = SystemIdentify.Authorization,
                Type = SecuritySchemeType.Http,
                Scheme = SystemIdentify.Bearer,
                BearerFormat = SystemIdentify.Jwt,
                In = ParameterLocation.Header,
            });

            //讓所有API套用JWT驗證
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SystemIdentify.Bearer
                        }
                    },
                    new string[] {}
                }
            });
        });

        var app = builder.Build();

        //基礎開發工具與面板
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHangfireDashboard("/hangfire");

        //日誌與異常處理
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionMiddleware>();

        //路由與 CORS
        app.UseRouting();
        app.UseCors("MyReactAppPolicy"); //必須放在MapControllers之前

        // HTTPS 與 安全性
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        //處理LOG的TraceId
        app.UseMiddleware<TraceContextMiddleware>();

        //初始化資料庫與資料
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await DbInitializer.Initialize(services);
        }

        app.MapControllers();

        app.MapHub<LogHub>("/hubs/log"); //SignalR Hub的路徑

        app.Run();
    }
}