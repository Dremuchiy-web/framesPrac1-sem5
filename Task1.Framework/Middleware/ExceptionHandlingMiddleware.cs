using System.Text.Json;
using Task1.Framework.Contracts;
using Task1.Framework.Domain;

namespace Task1.Framework.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var requestId = context.Items.TryGetValue(RequestContext.RequestIdItemKey, out var rid)
                ? (rid?.ToString() ?? string.Empty)
                : string.Empty;

            var (statusCode, code, message, logLevel) = ex switch
            {
                DomainValidationException dve => (StatusCodes.Status400BadRequest, dve.Code, dve.Message, LogLevel.Information),
                NotFoundException nfe => (StatusCodes.Status404NotFound, nfe.Code, nfe.Message, LogLevel.Information),
                _ => (StatusCodes.Status500InternalServerError, ErrorCodes.UnhandledError, "Unexpected server error.", LogLevel.Error),
            };

            _logger.Log(logLevel, ex, "Request failed (requestId={RequestId}, code={Code})", requestId, code);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new ApiErrorResponse(code, message, requestId);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}

