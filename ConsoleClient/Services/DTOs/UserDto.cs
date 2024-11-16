namespace ConsoleClient.Services.DTOs;

public class UserDto
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; }

    public override string ToString()
    {
        return $"Username: {Username}, Role: {Role}";
    }
}