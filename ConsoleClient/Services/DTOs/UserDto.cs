using System.Text;
using Microsoft.Extensions.Primitives;

namespace ConsoleClient.Services.DTOs;

public class UserDto
{
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();

    public override string ToString()
    {
        StringBuilder b = new StringBuilder();
        b.Append($"Username: {Username}; Roles: ");
        b.Append('[');
        for (var i = 0; i < Roles.Count; i++)
        {
            var role = Roles[i];
            if (i == Roles.Count - 1)
            {
                b.Append($"{role}");
            }
            else
            {
                b.Append($"{role}, ");
            }
        }

        b.Append(']');
        return b.ToString();
    }
}