using System.Net;
using System.Text.Json;
using JudicialEvidence.Api.Services;

namespace JudicialEvidence.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ServiceException ex)
        {
            _logger.LogWarning(ex, "业务校验失败: {Message}", ex.Message);
            await WriteAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未处理异常");
            await WriteAsync(context, (int)HttpStatusCode.InternalServerError, "服务器内部错误");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload);
    }
}
