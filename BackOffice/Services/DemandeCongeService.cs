using BackOffice.Context;
using BackOffice.Models;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Services;

public class DemandeCongeService
{
    private readonly MyDbContext _context;

    public DemandeCongeService(MyDbContext context)
    {
        _context = context;
    }

    // CREATE demande
    public async Task<DemandeConge> CreerDemandeAsync(DemandeConge demande)
    {
        demande.NombreJour = CalculerJoursOuvres(demande.DateDebut, demande.DateFin);
        demande.Status = StatusEnum.pending;

        _context.DemandeConges.Add(demande);
        await _context.SaveChangesAsync();

        return demande;
    }

    // VALIDATION (fonctionnalité complexe)
    public async Task<bool> ValiderDemandeAsync(int idDemande)
    {
        var demande = await _context.DemandeConges
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.IdDmd == idDemande);

        if (demande == null)
            return false;

        var conge = await _context.SoldeConges
            .FirstOrDefaultAsync(c => c.IdEmploye == demande.UserId);

        if (conge == null || conge.SoldeRestant < demande.NombreJour)
            return false;

        // Décrémenter solde
        conge.SoldeRestant -= demande.NombreJour;
        demande.Status = StatusEnum.ok;

        await _context.SaveChangesAsync();
        return true;
    }

    // READ
    public async Task<List<DemandeConge>> GetDemandesAsync()
    {
        return await _context.DemandeConges
            .Include(d => d.User)
            .ToListAsync();
    }

    // DELETE
    public async Task<bool> SupprimerDemandeAsync(int id)
    {
        var demande = await _context.DemandeConges.FindAsync(id);
        if (demande == null) return false;

        _context.DemandeConges.Remove(demande);
        await _context.SaveChangesAsync();
        return true;
    }

    // Calcul jours ouvrés
    private int CalculerJoursOuvres(DateTime debut, DateTime fin)
    {
        int jours = 0;
        for (var date = debut.Date; date <= fin.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
            {
                jours++;
            }
        }

        return jours;
    }

    public async Task<(List<DemandeConge> Items, int TotalCount)>
        GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.DemandeConges
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.IdDmd)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}