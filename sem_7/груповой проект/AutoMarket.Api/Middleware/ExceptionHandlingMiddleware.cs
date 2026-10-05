using AutoMarket.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Api.Middleware;

/// <summary>
/// Глобальна обробка помилок: бізнес-виключення → відповідний HTTP-код,
/// все інше → 500 без деталей стеку для клієнта. Формат відповіді — ProblemDetails (RFC 7807).
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning("{Method} {Path} → {Status}: {Message}", context.Request.Method, context.Request.Path, ex.StatusCode, ex.Message);
            await WriteProblemAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Необроблена помилка на {Method} {Path}", context.Request.Method, context.Request.Path);
            var detail = env.IsDevelopment() ? ex.Message : "Внутрішня помилка сервера";
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, detail);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string detail)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrase(status),
            Detail = detail,
            Instance = context.Request.Path
        }, options: null, contentType: "application/problem+json");
    }

    private static string ReasonPhrase(int status) => status switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        _ => "Server Error"
    };
}
