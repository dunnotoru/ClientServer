using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServer.DAL;
using WebServer.DAL.DTOs;
using WebServer.DAL.Repositories.Abstractions;

namespace WebServer.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;
    
    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login([FromBody] CreateUserDto createUser)
    {
        _logger.LogInformation("Login endpoint");
        int id = _userRepository.Create(createUser);
        if (id < 0)
        {
            return BadRequest("User with this name already exists");
        }
        
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public IActionResult Patch(int id, [FromBody] PatchUserDto patchUserDto)
    {
        _logger.LogInformation("Patch user");
        User? user = _userRepository.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!_userRepository.Patch(id, patchUserDto))
        {
            return Problem();
        }

        return Ok(new
        {
            id
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Get all users");
        Dictionary<int, ResponseUserDto> users =  _userRepository.GetUsers().Select(pair =>
            {
                return new KeyValuePair<int, ResponseUserDto>(pair.Key, new ResponseUserDto
                {
                    Username = pair.Value.Username,
                    UserRole = pair.Value.UserRole,
                });
            })
        .ToDictionary();
        return Ok(users);
    }

    [Authorize(Policy = "AdminOrOwner")]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        User? user = _userRepository.GetById(id);
        if (user is null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }

    [Authorize(Policy = "AdminOrOwner")]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Delete user");
        User? user = _userRepository.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!_userRepository.Delete(id))
        {
            return Problem();
        }
        
        return Ok();
    }
}