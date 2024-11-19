using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServer.Auth;

namespace WebServer.Controllers;

[ApiController]
[Route("api/")]
public class ServiceController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServiceController(IHttpClientFactory httpClientFactory)
    {   
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("song")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.Scheme)]
    public async Task<IActionResult> GetSong()
    {
        HttpClient client = _httpClientFactory.CreateClient("Service");
        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync("definitely-not-rickroll");
        }
        catch (Exception)
        {
            return Problem();
        }
        
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Service error");
        }

        try
        {
            string result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem();
        }
    }
}