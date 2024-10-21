using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using WebServer.DAL.DTOs;
using WebServer.DAL.Repositories.Abstractions;

namespace WebServer.DAL.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<int, User> _users = new ConcurrentDictionary<int, User>();
    private readonly IPasswordHasher<User> _passwordHasher;
    private int _currentId = 0;

    public InMemoryUserRepository(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
        Create(new CreateUserDto
        {
            Username = "root",
            Password = "root",
        });
        Patch(0, new PatchUserDto
        {
            UserRole = Role.Admin
        });
    }
    
    public int Create(CreateUserDto createUserDto)
    {
        User user = new User
        {
            Username = createUserDto.Username,
            UserRole = Role.User,
        };

        if (_users.Any(pair => pair.Value.Username == user.Username))
        {
            return -1;
        }
            
        user.PasswordHash = _passwordHasher.HashPassword(user, createUserDto.Password);
        
        bool result = _users.TryAdd(_currentId++, user);
        if (result)
        {
            return _currentId - 1;
        }

        return -1;
    }

    public User? GetById(int id)
    {
        _users.TryGetValue(id, out User? user);
        return user;
    }

    public Tuple<int, User>? GetByUsername(string username)
    {
        KeyValuePair<int, User> a = _users.FirstOrDefault(u => u.Value.Username == username);
        return new Tuple<int, User>(a.Key, a.Value);
    }

    public Dictionary<int, User> GetUsers()
    {
        return _users.ToDictionary(u => u.Key, u => u.Value);
    }

    public bool Patch(int id, PatchUserDto patchUser)
    {
        if (!_users.ContainsKey(id))
        {
            return false;
        }
        
        _users[id].UserRole = patchUser.UserRole;
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

    public bool ValidatePassword(string username, string password)
    {
        Tuple<int, User>? pair = GetByUsername(username);
        if (pair is null)
        {
            return false;
        }
        
        User user = pair.Item2;
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash , password);
        return result == PasswordVerificationResult.Success;
    }
}