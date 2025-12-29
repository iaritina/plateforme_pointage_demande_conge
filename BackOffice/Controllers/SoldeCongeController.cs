using BackOffice.DTO;
using BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SoldeCongeController : Controller
{
    private readonly ILogger<SoldeCongeController> _logger;
    private readonly SoldeCongeService _service;
    private const int PageSize = 10;

    public SoldeCongeController(ILogger<SoldeCongeController> logger, SoldeCongeService service)
    {
        _logger = logger;
        _service = service;
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1)
    {
        var (items, totalCount) =
            await _service.GetAllPagedAsync(page, PageSize);

        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        return Ok(new
        {
            currentPage = page,
            totalPages,
            pageSize = PageSize,
            totalCount,
            items
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SoldeCongeDTO dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Bad request");
            return BadRequest(ModelState);
        }
            

        await _service.CreateAsync(dto.EmployeeId, dto.SoldeRestant);

        return CreatedAtAction(nameof(GetAll), new { page = 1 }, dto);
    }
}