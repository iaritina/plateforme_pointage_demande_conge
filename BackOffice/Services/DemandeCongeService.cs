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
    
    public async Task<bool> RefuserDemandeAsync(int idDemande)
    {
        var demande = await _context.DemandeConges
            .FirstOrDefaultAsync(d => d.IdDmd == idDemande);

        if (demande == null)
            return false;

        if (demande.Status == StatusEnum.ok || demande.Status == StatusEnum.ko)
            return false;

        demande.Status = StatusEnum.ko;

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

        // Parcourir de la date de départ à la date de retour
        for (var date = d.DateDebut.Date; date <= d.DateFin.Date; date = date.AddDays(1))
        {
            // Ignorer weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // CAS 1 : Même jour (départ et retour le même jour)
            if (d.DateDebut.Date == d.DateFin.Date)
            {
                if (!d.DebutApresMidi && !d.FinApresMidi)
                    return 0m;
            
                if (!d.DebutApresMidi && d.FinApresMidi)
                    return 0.5m;
            
                if (d.DebutApresMidi && d.FinApresMidi)
                    return 0m;
            
                return 0m;
            }

            // CAS 2 : Plusieurs jours
            // Premier jour (jour de départ)
            if (date == d.DateDebut.Date)
            {
                total += d.DebutApresMidi ? 0.5m : 1m;
            }
            else if (date == d.DateFin.Date)
            {
                total += d.FinApresMidi ? 0.5m : 0m;
            }
            // Jours intermédiaires
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