namespace WebServer.DAL;

public class CreateUserDTO
{
    public string Username { get; set; }
    public string Password { get; set; }
    public Role UserRole { get; set; }
}