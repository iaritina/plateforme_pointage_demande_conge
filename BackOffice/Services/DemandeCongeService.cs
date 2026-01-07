using Microsoft.EntityFrameworkCore;
using Shared.Context;
using Shared.models;

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
        demande.NombreJour = CalculerJoursOuvres(demande);
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
    private decimal CalculerJoursOuvres(DemandeConge d)
    {
        decimal total = 0;

        // parcourt chaque date en ajoutant +1
        for (var date = d.DateDebut.Date; date <= d.DateFin.Date; date = date.AddDays(1))
        {
            // Ignore samedi / dimanche
            if (date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Jour unique
            if (d.DateDebut.Date == d.DateFin.Date)
            {
                if (d.DebutApresMidi && !d.FinApresMidi)
                    total += 0.5m;
                else
                    total += 1m;

                break;
            }

            // Premier jour
            if (date == d.DateDebut.Date)
            {
                total += d.DebutApresMidi ? 0.5m : 1m;
            }
            // Dernier jour
            else if (date == d.DateFin.Date)
            {
                total += d.FinApresMidi ? 1m : 0.5m;
            }
            // Jours intermédiaires, les jours entre debut et fin
            else
            {
                total += 1m;
            }
        }

        return total;
    }

    public async Task<(List<DemandeConge> Items, int TotalCount)>
        GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.DemandeConges
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.IdDmd)
            .Include(d => d.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<DemandeConge>> GetDemandesAsync(int userId)
    {
        return await _context.DemandeConges
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.IdDmd)
            .ToListAsync();
    }
}