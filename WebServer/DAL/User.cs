namespace WebServer.DAL;

public enum Role
{
    Admin = 0,
    User
}

public class User
{
    public string PasswordHash = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Role UserRole { get; set; } = Role.User;
}