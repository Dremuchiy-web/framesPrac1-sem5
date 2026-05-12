using System.Diagnostics;

namespace Task1.Framework.Middleware;

public sealed class RequestTimingMiddleware : IMiddleware
{
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(ILogger<RequestTimingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            var durationMs = sw.Elapsed.TotalMilliseconds;
            context.Items[RequestContext.DurationMsItemKey] = durationMs;

            var requestId = context.Items.TryGetValue(RequestContext.RequestIdItemKey, out var rid)
                ? rid?.ToString()
                : null;

            _logger.LogInformation(
                "Request completed in {DurationMs:0.###} ms (requestId={RequestId})",
                durationMs,
                requestId);
        }
    }
}

