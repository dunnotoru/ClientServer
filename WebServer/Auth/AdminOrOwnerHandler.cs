using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebServer.DAL.Entity;

namespace WebServer.Auth;

public class AdminOrOwnerHandler : AuthorizationHandler<AdminOrOwnerRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrOwnerRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        if (!httpContext.Request.RouteValues.TryGetValue("id", out object? id))
        {
            return Task.CompletedTask;
        }
        
        int userId = Convert.ToInt32(id);
        
        if (!(context?.User?.Identity?.IsAuthenticated ?? true))
        {
            return Task.CompletedTask;
        }
        
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}