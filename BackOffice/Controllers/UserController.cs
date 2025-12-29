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

    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetUserPaginated(page, pageSize);
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Unexpected error: {ex.Message}"
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.LastName);
            }

            var createdUser = await _service.CreateUser(user);
            return CreatedAtAction(nameof(GetAll), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur lors de la création de l'utilisateur : {ex.Message}");
        }
    }

    [HttpPost("importCsv")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier sélectionné.");

        try
        {
            using var stream = new StreamReader(file.OpenReadStream());
            string? headerLine = await stream.ReadLineAsync();

            var createdUsers = new List<User>();
            while (!stream.EndOfStream)
            {
                var line = await stream.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');

                var user = new User
                {
                    FirstName = values[0].Trim(),
                    LastName = values[1].Trim(),
                    Email = values[2].Trim(),
                    Phone = values[3].Trim(),
                    HiringDate = DateTime.TryParse(values[4].Trim(), out var date) ? date : DateTime.UtcNow,
                    Role = values.Length > 5 ? values[5].Trim() : "User"
                };

                var created = await _service.CreateUser(user);
                createdUsers.Add(created);
            }

            return Ok(new
            {
                success = true,
                message = $"{createdUsers.Count} utilisateurs importés avec succès.",
                data = createdUsers
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de l'import : {ex.Message}"
            });
        }


}

}