using Microsoft.AspNetCore.Mvc;
using WebServer.DAL;
using WebServer.DAL.Repositories.Abstractions;
using WebServer.Services.Abstractions;

namespace WebServer.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;
    private readonly IBase64Encoder _base64Encoder;
    
    public UserController(IUserRepository userRepository, ILogger<UserController> logger, IBase64Encoder base64Encoder)
    {
        _userRepository = userRepository;
        _logger = logger;
        _base64Encoder = base64Encoder;
    }
    
    [HttpPost]
    public IActionResult Post([FromBody] CreateUserDto createUser)
    {
        int id = _userRepository.Create(createUser);
        if (id < 0)
        {
            return BadRequest("User with this name already exists");
        }
        
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPatch("{id:int}")]
    public IActionResult Patch(int id, [FromBody] PatchUserDto patchDoc, [FromHeader] string? authHeader)
    {
        IActionResult? result = AuthorizeAdmin(authHeader);
        if (result is not null)
        {
            return result;
        }
        
        User? user = _userRepository.GetById(id);
        if (user is null)
        {
            return NotFound();
        }

        if (_userRepository.Patch(id, patchDoc))
        {
            return Problem();
        }

        return Ok(new
        {
            id = id
        });
    }

    [HttpGet]
    public IActionResult Get([FromHeader(Name = "Authorization")] string? authHeader)
    {
        IActionResult? result = AuthorizeAdmin(authHeader);
        if (result is not null)
        {
            return result;
        }
        
        Dictionary<int, User> users =  _userRepository.GetUsers();
        return Ok(new
        {
            Users = users
        });
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id, [FromHeader(Name = "Authorization")] string? authHeader)
    {
        IActionResult? result = AuthorizeAdminOrUser(id, authHeader);
        if (result is not null)
        {
            return result;
        }
        
        User? user = _userRepository.GetById(id);
        if (user is null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id, [FromHeader(Name = "Authorization")] string? authHeader)
    {
        IActionResult? result = AuthorizeAdminOrUser(id, authHeader);
        if (result is not null)
        {
            return result;
        }
        
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

    private IActionResult? AuthorizeAdmin(string? authHeader)
    {
        if (authHeader is null)
        {
            Response.Headers.Append("WWW-Authenticate", "Basic Realm=\"Access to the api\"");
            return BadRequest();
        }
        
        string username;
        string password;
        try
        {
            (username, password) = GetUsernameAndPassword(authHeader);
        }
        catch (FormatException ex)
        {
            return BadRequest("Invalid username or password: ");
        }
        
        User? storedUser = _userRepository.GetByUsername(username);
        if (storedUser is null)
        {
            return NotFound("User with this name is not found");
        }
        if (_userRepository.ValidatePassword(username, password))
        {
            return Unauthorized();
        }
        if (storedUser.UserRole != Role.Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return null;
    }

    private IActionResult? AuthorizeAdminOrUser(int requiredId, string? authHeader)
    {
        if (authHeader is null)
        {
            Response.Headers.Append("WWW-Authenticate", "Basic Realm=\"Access to the api\"");
            return BadRequest();
        }
        
        string username;
        string password;
        try
        {
            (username, password) = GetUsernameAndPassword(authHeader);
        }
        catch (FormatException ex)
        {
            return BadRequest("Invalid username or password: ");
        }
        
        User? storedUser = _userRepository.GetByUsername(username);
        if (storedUser is null)
        {
            return NotFound("User with this name is not found");
        }
        if (_userRepository.ValidatePassword(username, password))
        {
            return Unauthorized();
        }
        
        User? requiredUser = _userRepository.GetById(requiredId);

        if (storedUser.UserRole == Role.Admin)
        {
            return null;
        }
        
        if (requiredUser is null || requiredUser.Username != storedUser.Username)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return null;
    }

    private Tuple<string, string> GetUsernameAndPassword(string base64EncodedUserData)
    {
        string[] basicSplit = base64EncodedUserData.Split(" "); 
        if (basicSplit.Length != 2)
        {
            throw new FormatException("Invalid username or password");
        }
        string data = _base64Encoder.Decode(basicSplit[1]);
        string[] s = data.Split(":");
        return new Tuple<string, string>(s[0], s[1]);
    } 
}