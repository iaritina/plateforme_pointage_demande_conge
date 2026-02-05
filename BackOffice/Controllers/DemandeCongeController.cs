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
    public async Task<IActionResult> GetAll(int page = 1)
    {
        var (items, totalCount) =
            await _demandeService.GetAllPagedAsync(page, PageSize);

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
}