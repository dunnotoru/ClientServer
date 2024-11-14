using WebServer.DAL.Entity;

namespace WebServer.DAL.DTOs;

public class PatchUserDto
{
    public Role UserRole { get; set; }
}