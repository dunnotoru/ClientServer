using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebServer.DAL;
using WebServer.DAL.Repositories.Abstractions;

namespace WebServer.Auth;

public class AdminOrOwnerHandler : AuthorizationHandler<AdminOrOwnerRequirement>
{
    private readonly IUserRepository _userRepository;

    public AdminOrOwnerHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

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
    
        User? a = _userRepository.GetById(userId);
        if (a is null)
        {
            return Task.CompletedTask;
        }
        
        if (!context.User.HasClaim(ClaimTypes.NameIdentifier, a.Username))
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}