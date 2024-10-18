using System.Net;
using ConsoleClient.Services;
using Spectre.Console;

namespace ConsoleClient;

public static class Program
{
    public static void Main(string[] args)
    {
        AnsiConsole.MarkupLine("Client starting...");
        IPAddress? ip = null;
        AnsiConsole.Prompt(
            new TextPrompt<string>("Input server IPv4 address:")
                .Validate(s => IPAddress.TryParse(s, out ip)
        ));
        
        int port = AnsiConsole.Prompt(
            new TextPrompt<int>("Input server port:")
                .Validate(p => p switch
                {
                    < 0 => ValidationResult.Error("Invalid port number, must be in range from 0 to 65535"),
                    > 0 and < 65535 => ValidationResult.Success(),
                    > 65535 => ValidationResult.Error("Invalid port number, must be in range from 0 to 65535"),
                }));
        
        AnsiConsole.Clear();
        ClientController controller = new ClientController(new Base64Encoder(), new UserService(new IPEndPoint(ip!, port)));
        controller.Run();
    }
}