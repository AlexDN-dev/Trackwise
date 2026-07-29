using Trackwise.Application.Admin;

namespace Trackwise.API.Middleware;

public sealed class BannedIpMiddleware
{
    private readonly RequestDelegate _next;

    public BannedIpMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IBannedIpService bannedIps)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ip) && await bannedIps.IsBannedAsync(ip, context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access forbidden.");
            return;
        }

        await _next(context);
    }
}