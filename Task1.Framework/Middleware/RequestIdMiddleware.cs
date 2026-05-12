namespace Task1.Framework.Middleware;

public sealed class RequestIdMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestId = context.Request.Headers.TryGetValue(RequestContext.RequestIdHeaderName, out var existing) &&
                        !string.IsNullOrWhiteSpace(existing.ToString())
            ? existing.ToString().Trim()
            : Guid.NewGuid().ToString("N");

        context.Items[RequestContext.RequestIdItemKey] = requestId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[RequestContext.RequestIdHeaderName] = requestId;
            return Task.CompletedTask;
        });

        return next(context);
    }
}

