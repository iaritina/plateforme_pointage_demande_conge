using System.Security.Claims;
using FrontOffice.Auth;
using FrontOffice.Service;
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
    [JwtAuthorize]
    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var demandes = await _demandeCongeService.GetUserDemandesAsync(userId);
        return View(demandes);
    }

    public IActionResult Creer()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [JwtAuthorize]
    public async Task<IActionResult> Creer(DemandeCongeCreateViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var demande = new DemandeConge
            {
                UserId = userId,
                DateDebut = model.DateDebut,
                DateFin = model.DateFin,
                Motif = model.Motif,
                DebutApresMidi = model.DebutApresMidi,
                FinApresMidi = model.FinApresMidi,
                decisionYear =  model.decisionYear,
            };

            await _demandeCongeService.CreerDemandeAsync(demande, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex) // ex: "0 jour ouvré"
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (ArgumentException ex) // ex: date fin < date début
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception)
        {
            // Erreur technique -> message générique
            ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
            return View(model);
        }
    }
}