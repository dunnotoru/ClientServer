using ConsoleClient.Services.DTOs;

namespace ConsoleClient.Services.Abstractions;

public interface IUserService
{
    int Create(CreateUserDto createUserDto);
    IDictionary<int, UserDto> GetUsers(string basicAuthToken);
    UserDto? GetUserById(string basicAuthToken, int userId);
    UserDto? GetUserByUsername(string basicAuthToken, string username);
    void DeleteUser(string basicAuthToken, int userId);
    int PatchUser(string basicAuthToken, int userId, PatchUserDto userDto);
}