using BackOffice.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.models;

namespace BackOffice.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RegistrationController : Controller
{
    private readonly RegistrationService _service;
    
    public RegistrationController(RegistrationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Registration request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _service.Create(request.UserId, request.Status);
            return Ok(new
            {
                success = true,
                message = "Registration created successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
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
}