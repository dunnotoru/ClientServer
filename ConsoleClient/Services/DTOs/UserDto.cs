namespace ConsoleClient.Services.DTOs;

public class UserDto
{
    public string Username { get; set; }
    public int UserRole { get; set; }

    public override string ToString()
    {
        return $"Username: {Username}, " + (UserRole == 0 ? "Role: Admin" : "Role: User");  
    }
}