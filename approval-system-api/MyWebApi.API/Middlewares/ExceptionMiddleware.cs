using Microsoft.EntityFrameworkCore;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Domain.Constants;
using MyWebApi.Domain.Entities;
using System.Globalization;
using System.Text.Json;

namespace MyWebApi.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var status = 0;
        var errorMessage = string.Empty;

        //處理非同步async/await發生錯誤時，會包在AggregateException內的情況，取出真正的例外
        if (ex is AggregateException aggregateEx)
        {
            ex = aggregateEx.Flatten().InnerExceptions.First();
        }

        switch (ex)
        {
            case FluentValidation.ValidationException fEx:
                status = StatusCodes.Status400BadRequest;
                //將所有錯誤訊息串接起來，讓前端一次看到所有錯誤
                errorMessage = string.Join("; ", fEx.Errors.Select(e => e.ErrorMessage));
                break;
            case ArgumentException _:
                status = StatusCodes.Status400BadRequest;
                errorMessage = ex.Message;
                break;
            case KeyNotFoundException _:
                status = StatusCodes.Status404NotFound;
                errorMessage = ex.Message;
                break;
            case UnauthorizedAccessException _:
                status = StatusCodes.Status401Unauthorized;
                errorMessage = ExceptionMessages.Status401Unauthorized;
                break;
            case DbUpdateConcurrencyException _:
                status = StatusCodes.Status409Conflict;
                errorMessage = ExceptionMessages.Status409Conflict;
                break;
            default:
                status = StatusCodes.Status500InternalServerError;
                errorMessage = _env.IsDevelopment() ? ex.ToString() : ExceptionMessages.Status500InternalServerError;
                break;
        }

        //根據狀態碼記錄不同的LOG
        switch (status)
        {
            case StatusCodes.Status400BadRequest:
                _logger.LogInformation("[請求錯誤] 400 Bad Request: {Path}, Message: {Msg}",
                    context.Request.Path,
                    errorMessage);
                break;

            case StatusCodes.Status401Unauthorized:
                _logger.LogWarning("[安全警示] 發現未授權存取 (401)。IP: {Ip}, Path: {Path}, Message: {Msg}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    errorMessage);
                break;

            case StatusCodes.Status404NotFound:
                _logger.LogInformation("[查無資源] 404 Not Found: {Path}, Message: {Msg}",
                    context.Request.Path,
                    errorMessage);
                break;

            case var s when s >= 400 && s < 500:
                _logger.LogInformation("[客戶端錯誤] {Status}: {Path}, Msg: {Msg}", s, context.Request.Path, errorMessage);
                break;

            case StatusCodes.Status500InternalServerError:
            default:
                _logger.LogError(ex, "[系統異常] Path: {Path}", context.Request.Path);
                break;
        }

        var problem = new BaseRes
        {
            IsSuccess = false,
            Message = errorMessage,
            Status = status,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var payload = JsonSerializer.Serialize(problem, options);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(payload);
    }
}
