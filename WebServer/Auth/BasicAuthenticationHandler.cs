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

public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    private readonly BasicAuthenticationService _basicAuthenticationService;
    private IUserRepository _userRepository;

    public BasicAuthenticationHandler(
        IOptionsMonitor<BasicAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        BasicAuthenticationService basicAuthenticationService,
        IUserRepository userRepository) : base(options, logger, encoder)
    {
        _basicAuthenticationService = basicAuthenticationService;
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
        User? storedUser = _userRepository.GetByUsername(s[0]);
        if (storedUser is null)
        {
            return await Task.FromResult(AuthenticateResult.Fail("User does not exist"));
        }

        if (!_userRepository.ValidatePassword(storedUser.Username, s[1]))
        {
            return await Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
        }

        Claim[] claims;
        if (storedUser.UserRole == Role.Admin)
        {
            claims = [new Claim(ClaimTypes.Role, "Admin")];
        }
        else
        {
            claims = [new Claim(ClaimTypes.Role, "User")];
        }
        
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, BasicAuthenticationDefaults.Scheme);
        AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties(), BasicAuthenticationDefaults.Scheme);
        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.WWWAuthenticate = "Basic";
        return Task.CompletedTask;
    }
}