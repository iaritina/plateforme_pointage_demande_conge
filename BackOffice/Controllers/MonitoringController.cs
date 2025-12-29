using BackOffice.Services;
using BackOffice.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : Controller
{
    private readonly MonitoringService _service;

    public MonitoringController(MonitoringService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<ActionResult<MonitoringViewModel>> GetStatus()
    {
        try
        {
            var result = await _service.GetStatusAsync();
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
                message = "Une erreur est survenue lors de la récupération du statut des utilisateurs.",
                error = ex.Message
            });
        }
    }
    
    
}