using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServer.DAL.DTOs;
using WebServer.DAL.Entity;

namespace WebServer.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly UserManager<User> _userManager;
    
    public UserController(ILogger<UserController> logger,
        IPasswordHasher<User> passwordHasher, 
        UserManager<User> userManager)
    {
        _logger = logger;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUser)
    {
        _logger.LogInformation("Login endpoint");
        User user = new User
        {
            Username = createUser.Username,
        };
        
        IdentityResult result = await _userManager.CreateAsync(
            user,
            _passwordHasher.HashPassword(user, createUser.Password)
            );

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }
        
        return CreatedAtAction(nameof(Get), new { user.Id }, new { user.Id });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchUserDto patchUserDto)
    {
        _logger.LogInformation("Patch user");
        
        User? storedUser = _userManager.Users.SingleOrDefault(u => u.Id == id);
        if (storedUser is null)
        {
            return NotFound();
        }

        storedUser.UserRole = patchUserDto.UserRole;
        await _userManager.AddToRoleAsync(storedUser, storedUser.UserRole.ToString());
        
        IdentityResult result = await _userManager.UpdateAsync(storedUser);

        if (!result.Succeeded)
        {
            return Problem();
        }
    
        return Ok(new
        {
            storedUser.Id
        });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Get all users");

        var result = await _userManager.Users
            .Select(user => new
            {
                user.Username,
                user.UserRole
            }).ToListAsync();
        
        return Ok(result);
    }
    
    [Authorize(Policy = "AdminOrOwner")]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        _logger.LogInformation("Get user");
        User? storedUser = _userManager.Users.SingleOrDefault(u => u.Id == id);
        if (storedUser is null)
        {
            return NotFound();
        }
        
        return Ok(storedUser);
    }
    
    [Authorize(Policy = "AdminOrOwner")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete user");
        User? storedUser = _userManager.Users.SingleOrDefault(u => u.Id == id);
        if (storedUser is null)
        {
            return NotFound();
        }
        
        IdentityResult result = await _userManager.DeleteAsync(storedUser);

        if (!result.Succeeded)
        {
            return Problem();
        }
    
        return Ok();
    }
}