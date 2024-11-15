using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebServer.DAL.Entity;

namespace WebServer.Auth;

public class AdminOrOwnerHandler : AuthorizationHandler<AdminOrOwnerRequirement>
{
    private readonly UserManager<User> _userManager;

    public AdminOrOwnerHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrOwnerRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        if (!httpContext.Request.RouteValues.TryGetValue("id", out object? id))
        {
            return;
        }
        
        int userId = Convert.ToInt32(id);
        if (httpContext.User.Identity is not ClaimsIdentity identity)
        {
            return;
        }
        
        if (!identity.IsAuthenticated)
        {
            return;
        }
        
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        User? storedUser = await _userManager.FindByIdAsync(userId.ToString());
        if (storedUser is null)
        {
            return;
        }

        if (context.User.HasClaim(
                ClaimTypes.NameIdentifier,
                storedUser.UserName ?? throw new InvalidOperationException("username is missing")))
        {
            context.Succeed(requirement);
        }
    }
}