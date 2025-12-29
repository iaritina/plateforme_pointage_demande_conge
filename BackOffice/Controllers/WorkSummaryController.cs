using Microsoft.AspNetCore.Mvc;
using BackOffice.Services;

namespace BackOffice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkSummaryController : Controller
{
    private readonly WorkSummaryService _service;
    
    public WorkSummaryController(WorkSummaryService service)
    {
        _service = service;
    } 
    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var summary = await _service.GetWorkSummary(startDate, endDate);
            return Ok(new
            {
                success = true,
                data = summary
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur: {ex.Message}"
            });
        }
    }

}