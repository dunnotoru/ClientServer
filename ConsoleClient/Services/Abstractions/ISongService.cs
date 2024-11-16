namespace ConsoleClient.Services.Abstractions;

public interface ISongService
{
    string GetChorus(string basicAuthToken);
}