namespace ConsoleClient.Services.DTOs;

public class CreateUserDto(string username, string password)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}