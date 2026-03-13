using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyWebApi.Application.Services;
using MyWebApi.Application.Validator;
using MyWebApi.Domain.Constants;

namespace MyWebApi.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //應用層的服務
            services.AddScoped<ApprovalService>();

            //FluentValidation
            services.AddValidatorsFromAssemblyContaining<ApplicationFormAdd_ReqValidator>();

            return services;
        }
    }
}
