using System.Net;
using System.Text.Json;
using HZ.IDTS.DigitalTwin.Contracts.Common;

namespace HZ.IDTS.DigitalTwin.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception exception)
        {
            await WriteUnhandledExceptionAsync(context, exception);
        }
    }

    private async Task WriteUnhandledExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        _logger.LogError(exception, "Unhandled API exception.");

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object?>.Failure(
            ErrorCode.InternalError,
            "Internal server error.",
            null);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions),
            context.RequestAborted);
    }
}
