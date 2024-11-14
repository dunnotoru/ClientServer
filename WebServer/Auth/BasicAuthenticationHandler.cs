using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace WebServer.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        StringValues authorizationHeader = Context.Request.Headers["Authorization"];

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "usernamelmao"),
            new Claim(ClaimTypes.Role, "HELL IT WE BALL"),
        };
        
        ClaimsIdentity identity = new ClaimsIdentity(claims, BasicAuthenticationDefaults.Scheme);
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