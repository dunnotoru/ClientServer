using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServer.DAL;
using WebServer.DAL.DTOs;
using WebServer.DAL.Repositories.Abstractions;
using WebServer.Services.Abstractions;

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
        int id = _userRepository.Create(createUser);
        if (id < 0)
        {
            return BadRequest("User with this name already exists");
        }
        
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [Authorize]
    [HttpPatch("{id:int}")]
    public IActionResult Patch(int id, [FromBody] PatchUserDto patchUserDto)
    {
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
            id = id
        });
    }

    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
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

    [Authorize]
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

    [Authorize]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
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