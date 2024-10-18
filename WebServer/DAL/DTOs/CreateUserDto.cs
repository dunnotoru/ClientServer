namespace WebServer.DAL.DTOs;

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}