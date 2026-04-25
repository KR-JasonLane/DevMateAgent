namespace DevPilotAgent.Api.Middleware;

using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "잘못된 요청: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("이미 진행 중인 분석"))
        {
            _logger.LogWarning(ex, "동시 분석 충돌: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("분석 이후 변경"))
        {
            _logger.LogWarning(ex, "파일 변경 충돌: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "파일 없음: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "서버 오류 발생");
            var message = _environment.IsDevelopment() ? ex.ToString() : "내부 서버 오류가 발생했습니다.";
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, message);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(response);
    }
}
