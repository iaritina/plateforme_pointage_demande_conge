using BackOffice.DTO;
using BackOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackOffice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemandeCongeController : Controller
{
    private readonly DemandeCongeService _demandeService;

    private const int PageSize = 5;

    public DemandeCongeController(
        DemandeCongeService demandeCongeService)
    {
        _demandeService = demandeCongeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DemandeCongeQueryDTO q)
    {
        if (q.Page < 1) q.Page = 1;
        if (q.PageSize < 1) q.PageSize = 10;
        if (q.PageSize > 200) q.PageSize = 200; // limite sécurité

        var result = await _demandeService.SearchPagedAsync(q);

        return Ok(new
        {
            currentPage = q.Page,
            totalPages = (int)Math.Ceiling(result.TotalCount / (double)q.PageSize),
            pageSize = q.PageSize,
            totalCount = result.TotalCount,
            items = result.Items
        });
    }

    [HttpPost("valider/{id}")]
    public async Task<IActionResult> ValiderDemande(int id)
    {
        var success = await _demandeService.ValiderDemandeAsync(id);

        if (!success)
        {
            return BadRequest(new
            {
                message = "La demande ne peut pas être validée (solde insuffisant ou demande inexistante)."
            });
        }

        return Ok(new
        {
            message = "Demande de congé validée avec succès."
        });
    }
    
    [HttpPost("refuser/{id}")]
    public async Task<IActionResult> RefuserDemande(int id)
    {
        var success = await _demandeService.RefuserDemandeAsync(id);

        if (!success)
        {
            return BadRequest(new
            {
                message = "La demande ne peut pas être refusée (inexistante ou déjà traitée)."
            });
        }

        return Ok(new
        {
            message = "Demande de congé refusée avec succès."
        });
    }
}