using WebServer.DAL.DTOs;

namespace WebServer.DAL.Repositories.Abstractions;

public interface IUserRepository
{
    int Create(CreateUserDto createUser);
    User? GetById(int id);
    User? GetByUsername(string username);
    Dictionary<int, User> GetUsers();
    bool Patch(int id, PatchUserDto patchUser);
    bool Delete(int id);
    bool ValidatePassword(string username, string password);
}