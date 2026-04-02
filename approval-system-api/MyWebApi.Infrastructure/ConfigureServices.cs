using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyWebApi.Application.Interfaces;
using MyWebApi.Domain.Constants;
using MyWebApi.Domain.Interfaces;
using MyWebApi.Infrastructure.Authentication;
using MyWebApi.Infrastructure.Context;
using MyWebApi.Infrastructure.Repositories;
using MyWebApi.Infrastructure.Services;
using System.Text;

namespace MyWebApi.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConfigKeys.DefaultConnection);

            //HTTP服務存取器
            services.AddHttpContextAccessor();

            //資料庫連線
            services.AddDbContext<ApprovalSystemContext>(options =>
                options.UseSqlServer(connectionString));

            //Dapper連線
            services.AddScoped<ISqlConnectionFactory>(provider =>
                new SqlConnectionFactory(connectionString));

            //通用Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IApplicationFormRepository, ApplicationFormRepository>();

            //Auth服務
            services.AddScoped<IAuthService, AuthService>();

            //Email服務
            services.AddScoped<IEmailService, EmailService>();

            //Hangfire服務
            services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString));
            services.AddHangfireServer();
            services.AddScoped<IJobScheduler, HangfireJobScheduler>();

            services.AddHangfireServer();

            //JWT設定
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, //檢查Token是否過期
                    ValidateIssuerSigningKey = true, //檢查簽章是否正確

                    ValidIssuer = configuration[ConfigKeys.Jwt.Issuer],
                    ValidAudience = configuration[ConfigKeys.Jwt.Audience],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[ConfigKeys.Jwt.Key])),
                };
            });

            services.AddSignalR();

            return services;
        }
    }
}
