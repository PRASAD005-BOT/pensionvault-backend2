using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using PensionVault.Application.Services;
using PensionVault.Domain.Entities;

namespace PensionVault.API.Filters;

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly IAppDbContext _context;

    public AuditLogFilter(IAppDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Execute the action first
        var resultContext = await next();

        // 2. Check if action succeeded (no exception and HTTP status 2xx)
        if (resultContext.Exception == null && 
            resultContext.HttpContext.Response.StatusCode >= 200 && 
            resultContext.HttpContext.Response.StatusCode < 300)
        {
            var httpContext = resultContext.HttpContext;
            var method = httpContext.Request.Method;

            // 3. Only audit state-changing requests (POST, PUT, DELETE)
            if (method == "POST" || method == "PUT" || method == "DELETE")
            {
                var path = httpContext.Request.Path.Value ?? "";
                
                // Skip authentication paths except registration (which creates a user)
                if (path.Contains("/api/auth/login") || path.Contains("/api/auth/refresh"))
                    return;

                // 4. Get the authenticated user ID
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = null;

                if (Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }
                else if (path.Contains("/api/auth/register"))
                {
                    // Special case: During registration, the user was just created.
                    // We can try to extract the user email from the request body or let it slide.
                    // Let's just log it if we have an authenticated user.
                }

                if (userId.HasValue)
                {
                    // 5. Determine the entity and action names
                    var actionName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
                    var controllerName = context.ActionDescriptor.RouteValues["controller"] ?? "Unknown";

                    // Form a clean Action name (e.g. "CreateMember", "ReconcileRemittance")
                    var cleanAction = $"{actionName}{controllerName}";
                    if (cleanAction.EndsWith("sController")) 
                        cleanAction = cleanAction.Substring(0, cleanAction.Length - 11);
                    else if (cleanAction.EndsWith("Controller"))
                        cleanAction = cleanAction.Substring(0, cleanAction.Length - 10);

                    // Extract target record ID from route if present
                    string? recordId = null;
                    if (context.RouteData.Values.TryGetValue("id", out var idVal))
                    {
                        recordId = idVal?.ToString();
                    }

                    // Create the audit log entry
                    var auditLog = new AuditLog
                    {
                        AuditId = Guid.NewGuid(),
                        UserId = userId.Value,
                        Action = cleanAction,
                        EntityType = controllerName,
                        RecordId = recordId,
                        Timestamp = DateTime.UtcNow
                    };

                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
