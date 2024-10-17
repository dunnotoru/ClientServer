using System.Text;
using WebServer.Services.Abstractions;

namespace WebServer.Services;

public class Base64Encoder : IBase64Encoder
{
    public string Encode(string utf8Text)
    {
        byte[] utf8TextBytes = Encoding.UTF8.GetBytes(utf8Text);
        return Convert.ToBase64String(utf8TextBytes);
    }

    public string Decode(string base64EncodedString)
    {
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedString);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}