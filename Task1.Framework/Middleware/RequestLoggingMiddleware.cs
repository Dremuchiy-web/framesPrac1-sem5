namespace Task1.Framework.Middleware;

public sealed class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestId = context.Items.TryGetValue(RequestContext.RequestIdItemKey, out var rid)
            ? rid?.ToString()
            : null;

        _logger.LogInformation(
            "Request started {Method} {Path}{QueryString} (requestId={RequestId})",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            requestId);

        await next(context);

        var durationMs = context.Items.TryGetValue(RequestContext.DurationMsItemKey, out var d)
            ? d
            : null;

        _logger.LogInformation(
            "Request finished {StatusCode} {Method} {Path} (requestId={RequestId}, durationMs={DurationMs})",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            requestId,
            durationMs);
    }
}

