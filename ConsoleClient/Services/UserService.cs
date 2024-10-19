using System.Net;
using System.Net.Http.Json;
using ConsoleClient.Services.Abstractions;
using ConsoleClient.Services.DTOs;

namespace ConsoleClient.Services;

public class UserService(IPEndPoint endpoint) : IUserService
{
    private readonly HttpClient _client = new HttpClient
    {
        BaseAddress = new Uri($"http://{endpoint.Address}:{endpoint.Port}/api/", UriKind.Absolute),
        Timeout = TimeSpan.FromSeconds(15)
    };

    public int Create(CreateUserDto createUserDto)
    {
        JsonContent content = JsonContent.Create(new
        {
            username = createUserDto.Username,
            password = createUserDto.Password
        });
        
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("users", UriKind.Relative),
            Content = content
        };
        
        HttpResponseMessage response = _client.Send(request, new CancellationToken());
        response.EnsureSuccessStatusCode();
        ResponseUserDto? a = response.Content.ReadFromJsonAsync<ResponseUserDto>().Result;
        return a?.Id ?? -1;
    }

    public IDictionary<int, UserDto> GetUsers(string basicAuthToken)
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("users", UriKind.Relative),
            Headers = { { "Authorization", $"Basic {basicAuthToken}" } }
        };

        HttpResponseMessage response = _client.Send(request);
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<IDictionary<int, UserDto>>().Result ?? new Dictionary<int, UserDto>();
    }

    public UserDto? GetUserById(string basicAuthToken, int userId)
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"users/{userId}", UriKind.Relative),
            Headers = { { "Authorization", $"Basic {basicAuthToken}" } }
        };
        
        HttpResponseMessage response = _client.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<UserDto>().Result ?? null;
    }

    public UserDto? GetUserByUsername(string basicAuthToken, string username)
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"users/by_name/{username}", UriKind.Relative),
            Headers = { { "Authorization", $"Basic {basicAuthToken}" } }
        };
                        
        HttpResponseMessage response = _client.Send(request);
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<UserDto>().Result ?? null;
    }

    public int PatchUser(string basicAuthToken, int userId, PatchUserDto userDto)
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Patch,
            RequestUri = new Uri($"users/{userId}", UriKind.Relative),
            Headers = { { "Authorization", $"Basic {basicAuthToken}" } },
            Content = JsonContent.Create(userDto)
        };
        
        HttpResponseMessage response = _client.Send(request);
        response.EnsureSuccessStatusCode();
        ResponseUserDto? user = response.Content.ReadFromJsonAsync<ResponseUserDto>().Result;
        return user?.Id ?? -1;
    }
    
    public void DeleteUser(string basicAuthToken, int userId)
    {
        HttpRequestMessage request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"users/{userId}", UriKind.Relative),
            Headers = { { "Authorization", $"Basic {basicAuthToken}" } }
        };
        
        HttpResponseMessage response = _client.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();
    }
}