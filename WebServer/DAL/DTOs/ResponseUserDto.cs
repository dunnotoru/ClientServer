using WebServer.DAL.Entity;

namespace WebServer.DAL.DTOs;

public class ResponseUserDto
{
    public string Username { get; set; }
    public Role UserRole { get; set; }
}