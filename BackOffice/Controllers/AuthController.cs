using BackOffice.Services;
using BackOffice.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly AuthService _service;

    public AuthController(AuthService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _service.AuthenticateAsync(
                request.Email,
                request.Password
            );
            
            Console.WriteLine(token);
            
            if(token == null)
                return Unauthorized(new {success = false, message = "Identifiants invalides"});
            
            return Ok(new
            {
                success = true,
                token
            });
        }
        catch 
        {
            return StatusCode(500, new { success = false, message = "Erreur serveur" });
        }
        
    }
    
}