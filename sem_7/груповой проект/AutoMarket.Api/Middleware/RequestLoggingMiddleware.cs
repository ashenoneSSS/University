using System.Diagnostics;

namespace AutoMarket.Api.Middleware;

/// <summary>Логує кожен запит: метод, шлях, статус і час виконання. Додає заголовок X-Request-Id.</summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public const string RequestIdHeader = "X-Request-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers[RequestIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..12];
        context.Response.Headers[RequestIdHeader] = requestId;

        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            logger.LogInformation("[{RequestId}] {Method} {Path}{Query} → {Status} за {Elapsed} мс",
                requestId, context.Request.Method, context.Request.Path, context.Request.QueryString,
                context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
