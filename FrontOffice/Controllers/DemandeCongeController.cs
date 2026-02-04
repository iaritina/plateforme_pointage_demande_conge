using System.Security.Claims;
using BackOffice.Services;
using FrontOffice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Shared.models;

namespace FrontOffice.Controllers;

public class DemandeCongeController : Controller
{
    private readonly ILogger<DemandeCongeController> _logger;
    private readonly DemandeCongeService _demandeCongeService;

    public DemandeCongeController(ILogger<DemandeCongeController> logger,
        DemandeCongeService demandeCongeService)
    {
        _logger = logger;
        _demandeCongeService = demandeCongeService;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var demandes = await _demandeCongeService.GetDemandesAsync(userId);
        return View(demandes);
    }
    
    public IActionResult Creer()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Creer(DemandeCongeCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var demande = new DemandeConge
        {
            UserId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            ),
            DateDebut = model.DateDebut,
            DateFin = model.DateFin,
            Motif = model.Motif,
            DebutApresMidi = model.DebutApresMidi,
            FinApresMidi = model.FinApresMidi
        };

        await _demandeCongeService.CreerDemandeAsync(demande);

        return RedirectToAction("Index");
    }
}