using System.Net;
using System.Net.Http.Json;
using ConsoleClient.Services.Abstractions;
using ConsoleClient.Services.DTOs;

namespace ConsoleClient.Services;

public class NonRandomSongService(IPEndPoint endpoint) : ISongService
{
    private readonly HttpClient _client = new HttpClient
    {
        BaseAddress = new Uri($"http://{endpoint.Address}:{endpoint.Port}/api/", UriKind.Absolute),
        Timeout = TimeSpan.FromSeconds(15)
    };
    
    public string GetChorus()
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("/song", UriKind.Relative),
        };
        
        HttpResponseMessage response = _client.Send(request);
        response.EnsureSuccessStatusCode();
        string? a = response.Content.ReadFromJsonAsync<string>().Result;
        return a ?? "error";
    }
}