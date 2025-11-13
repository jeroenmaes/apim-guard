using ApimGuard.Models;
using ApimGuard.Services;
using System.Diagnostics;

namespace ApimGuard.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Only audit non-static file requests and meaningful routes
            if (!context.Request.Path.StartsWithSegments("/_framework") &&
                !context.Request.Path.StartsWithSegments("/lib") &&
                !context.Request.Path.StartsWithSegments("/css") &&
                !context.Request.Path.StartsWithSegments("/js") &&
                !context.Request.Path.Value!.EndsWith(".map"))
            {
                try
                {
                    var auditEntry = new AuditEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        UserId = context.User?.Identity?.Name,
                        UserName = context.User?.Identity?.Name ?? "Anonymous",
                        Action = context.Request.Path.Value ?? string.Empty,
                        Controller = GetControllerName(context),
                        Method = context.Request.Method,
                        Path = context.Request.Path.Value ?? string.Empty,
                        StatusCode = context.Response.StatusCode,
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        AdditionalData = new Dictionary<string, string>
                        {
                            { "Duration", $"{stopwatch.ElapsedMilliseconds}ms" },
                            { "QueryString", context.Request.QueryString.Value ?? string.Empty }
                        }
                    };

                    await auditService.LogAsync(auditEntry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error logging audit entry");
                }
            }
        }
    }

    private static string GetControllerName(HttpContext context)
    {
        var routeData = context.GetRouteData();
        return routeData?.Values["controller"]?.ToString() ?? "Unknown";
    }
}
