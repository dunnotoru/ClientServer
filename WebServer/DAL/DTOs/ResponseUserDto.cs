using Microsoft.VisualBasic;

namespace WebServer.DAL.DTOs;

public class ResponseUserDto
{
    public string Username { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}