namespace WebServer.DAL;

public interface IUserRepository
{
    int Create(CreateUserDTO user);
    User? GetById(int id);
    User? GetByUsername(string username);
    Dictionary<int, User> GetUsers();
    bool Update(int id, User user);
    bool Delete(int id);
}