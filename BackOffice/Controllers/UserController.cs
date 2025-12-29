using BackOffice.Models;
using BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly UserService _service;
    
    public  UserController(UserService service)
    {
        _service = service;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _service.GetAllUserAsync();

            return Ok(new
            {
                success = true,
                data = users
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to retrieve users",
                error = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        try
        {
            // Retire Password de la validation du modèle
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.LastName);
            var createdUser = await _service.CreateUser(user);
            return CreatedAtAction(nameof(GetAll), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la création de l'utilisateur : {ex.Message}");
        }
    }


}