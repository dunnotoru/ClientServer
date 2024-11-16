using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServer.Auth;
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
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    
    public UserController(ILogger<UserController> logger,
        IPasswordHasher<User> passwordHasher, 
        UserManager<User> userManager, 
        RoleManager<IdentityRole<int>> roleManager)
    {
        _logger = logger;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUser)
    {
        _logger.LogInformation("Login endpoint");
        User user = new User
        {
            UserName = createUser.Username,
        };
        
        IdentityResult result = await _userManager.CreateAsync(
            user,
            _passwordHasher.HashPassword(user, createUser.Password)
            );

        await _userManager.AddToRoleAsync(user, "User");

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }
        
        return CreatedAtAction(nameof(Get), new { user.Id }, new { user.Id });
    }
    
    [Authorize(Roles = "Admin", AuthenticationSchemes = BasicAuthenticationDefaults.Scheme)]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchUserDto patchUserDto)
    {
        _logger.LogInformation("Patch user");
        
        User? storedUser = _userManager.Users.SingleOrDefault(u => u.Id == id);
        if (storedUser is null)
        {
            return NotFound();
        }

        bool exists = await _roleManager.RoleExistsAsync(patchUserDto.Role);
        if (!exists)
        {
            return BadRequest("Role does not exist");
        }
        
        await _userManager.AddToRoleAsync(storedUser, patchUserDto.Role);
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
    
    [Authorize(Roles = "Admin", AuthenticationSchemes = BasicAuthenticationDefaults.Scheme)]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Get all users");
        
        List<User> users = await _userManager.Users.ToListAsync();
        List<ResponseUserDto> userDtos = new List<ResponseUserDto>();
        foreach (User user in users)
        {
            userDtos.Add(new ResponseUserDto
            {
                Username = user.UserName ?? "unknown",
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        
        return Ok(userDtos);
    }
    
    [Authorize(Policy = "AdminOrOwner", AuthenticationSchemes = BasicAuthenticationDefaults.Scheme)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        _logger.LogInformation("Get user");
        User? storedUser = _userManager.Users.SingleOrDefault(u => u.Id == id);
        if (storedUser is null)
        {
            return NotFound();
        }
        
        return Ok(new ResponseUserDto
        {
            Username = storedUser.UserName ?? "unknown",
            Roles = await _userManager.GetRolesAsync(storedUser)
        });
    }
    
    [Authorize(Policy = "AdminOrOwner", AuthenticationSchemes = BasicAuthenticationDefaults.Scheme)]
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