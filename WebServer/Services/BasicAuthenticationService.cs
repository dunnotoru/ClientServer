using System.ComponentModel.DataAnnotations;
using System.Text;
using WebServer.DAL;
using WebServer.DAL.Repositories.Abstractions;

namespace WebServer.Services;

public class BasicAuthenticationService
{
    private readonly IUserRepository _userRepository;
    
    public BasicAuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public ValidationResult? Authenticate(string base64EncodedCredentials, out bool isAdmin)
    {
        isAdmin = false;
        byte[] bytes = Convert.FromBase64String(base64EncodedCredentials);
        string decodedCredentials = Encoding.UTF8.GetString(bytes);
        string[] s = decodedCredentials.Split(":");
        User? storedUser = _userRepository.GetByUsername(s[0]);
        if (storedUser is null)
        {
            return new ValidationResult("User does not exist");
        }

        if (!_userRepository.ValidatePassword(storedUser.Username, s[1]))
        {
            return new ValidationResult("Invalid username or password");
        }

        if (storedUser.UserRole == Role.Admin)
        {
            isAdmin = true;
        }
        
        return ValidationResult.Success;
    }
}