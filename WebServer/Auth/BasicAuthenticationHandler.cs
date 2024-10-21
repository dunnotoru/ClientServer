using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WebServer.DAL;
using WebServer.DAL.Repositories.Abstractions;
using WebServer.Services;

namespace WebServer.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserRepository _userRepository;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserRepository userRepository) : base(options, logger, encoder)
    {
        _userRepository = userRepository;
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
        Tuple<int, User>? storedData = _userRepository.GetByUsername(s[0]);
        if (storedData is null)
        {
            return await Task.FromResult(AuthenticateResult.Fail("User does not exist"));
        }
        
        User storedUser = storedData.Item2;

        if (!_userRepository.ValidatePassword(storedUser.Username, s[1]))
        {
            return await Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
        }

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, storedUser.Username),
            new Claim(ClaimTypes.Role, storedUser.UserRole.ToString()),
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