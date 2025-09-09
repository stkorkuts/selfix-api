using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Selfix.Infrastructure.Notifications;

public class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var header = httpContext.Request.Headers.Authorization.ToString();
        
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.InvariantCulture))
        {
            SetUnauthorizedResponse(httpContext);
            return false;
        }

        var encodedCredentials = header["Basic ".Length..].Trim();
        var credentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        
        var parts = credentials.Split(':', 2);
        if (parts.Length != 2)
        {
            SetUnauthorizedResponse(httpContext);
            return false;
        }

        var username = parts[0];
        var password = parts[1];

        // In production, get these from config
        // TODO: use config
        if (username == "admin" && password == "admin") 
        {
            return true;
        }
        
        SetUnauthorizedResponse(httpContext);
        return false;
    }
    
    private static void SetUnauthorizedResponse(HttpContext httpContext)
    {
        httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
        httpContext.Response.StatusCode = 401;
    }
}