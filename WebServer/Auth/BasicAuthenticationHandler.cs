using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WebServer.DAL.Entity;

namespace WebServer.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly UserManager<User> _userManager;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder, 
        UserManager<User> userManager) : base(options, logger, encoder)
    {
        _userManager = userManager;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        StringValues authorizationHeader = Context.Request.Headers["Authorization"];
        
        if (authorizationHeader.Count == 0)
        {
            return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        string value = authorizationHeader.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }
        if (!value.StartsWith("Basic "))
        {
            return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }

        value = value.Remove(0,6);
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(value);
        }
        catch (Exception ex)
        {
            return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
        
        string decodedCredentials = Encoding.UTF8.GetString(bytes);
        string[] s = decodedCredentials.Split(":");
        
        User? storedUser = await _userManager.FindByNameAsync(s[0]);
        if (storedUser is null)
        {
            return await Task.FromResult(AuthenticateResult.Fail("User does not exist"));
        }

        IList<string> roles = await _userManager.GetRolesAsync(storedUser);

        Claim[] claims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToArray(); 
        ClaimsIdentity identity = new ClaimsIdentity(
            claims,
            BasicAuthenticationDefaults.Scheme);
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(claimsPrincipal, BasicAuthenticationDefaults.Scheme);
        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.WWWAuthenticate = "Basic";
        return Task.CompletedTask;
    }
}