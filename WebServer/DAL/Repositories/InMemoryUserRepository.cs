using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;

namespace WebServer.DAL.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private ConcurrentDictionary<int, User> _users = new ConcurrentDictionary<int, User>();
    private readonly IPasswordHasher<User> _passwordHasher;
    private int _nextId = -1;

    public InMemoryUserRepository(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
        Create(new CreateUserDTO
        {
            Username = "root",
            Password = "root",
            UserRole = Role.Admin
        });
    }
    
    public int Create(CreateUserDTO createUserDto)
    {
        User user = new User()
        {
            Username = createUserDto.Username,
            UserRole = createUserDto.UserRole,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, createUserDto.Password);
        
        bool result = _users.TryAdd(_nextId++, user);
        if (result)
        {
            return _nextId;
        }

        return -1;
    }

    public User? GetById(int id)
    {
        _users.TryGetValue(id, out User? user);
        return user;
    }

    public User? GetByUsername(string username)
    {
        return _users.FirstOrDefault(u => u.Value.Username == username).Value;
    }

    public Dictionary<int, User> GetUsers()
    {
        return _users.ToDictionary(u => u.Key, u => u.Value);
    }

    public bool Update(int id, User user)
    {
        if (!_users.ContainsKey(id))
        {
            return false;
        }
        
        _users[id] = user;
        return true;
    }

    public bool Delete(int id)
    {
        if (!_users.ContainsKey(id))
        {
            return false;
        }
        
        return _users.TryRemove(id, out _);
    }
}