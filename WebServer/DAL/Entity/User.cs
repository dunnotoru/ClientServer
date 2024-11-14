using Microsoft.AspNetCore.Identity;

namespace WebServer.DAL.Entity;

public enum Role
{
    Admin = 0,
    User
}

public class User : IdentityUser<int> 
{
    public string Username { get; set; } = string.Empty;
    public Role UserRole { get; set; } = Role.User;
}