namespace WebServer.Services.Abstractions;

public interface IBase64Encoder
{
    string Encode(string utf8Text);
    string Decode(string base64EncodedString);
}