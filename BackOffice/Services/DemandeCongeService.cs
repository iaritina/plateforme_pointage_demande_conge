using BackOffice.DTO;
using BackOffice.utils;
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
    
    public async Task<PagedResult<DemandeCongeDto>> SearchPagedAsync(DemandeCongeQueryDTO q)
    {
        // sécurité
        if (q.Page < 1) q.Page = 1;
        if (q.PageSize < 1) q.PageSize = 10;
        if (q.PageSize > 200) q.PageSize = 200;

        IQueryable<DemandeConge> query = _context.DemandeConges
            .AsNoTracking()
            .Include(d => d.User);

        // filters
        if (q.UserId.HasValue)
            query = query.Where(d => d.UserId == q.UserId.Value);

        if (q.Status.HasValue)
            query = query.Where(d => d.Status == q.Status.Value);

        if (q.DateFrom.HasValue)
            query = query.Where(d => d.DateDebut >= q.DateFrom.Value);

        if (q.DateTo.HasValue)
            query = query.Where(d => d.DateDebut <= q.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(q.Motif))
            query = query.Where(d => d.Motif.Contains(q.Motif));
        
        if (!string.IsNullOrWhiteSpace(q.FullName))
        {
            var search = q.FullName.ToLower();

            query = query.Where(d =>
                (d.User.FirstName + " " + d.User.LastName).ToLower().Contains(search)
                || (d.User.LastName + " " + d.User.FirstName).ToLower().Contains(search)
            );
        }

        // sort (whitelist)
        var dirAsc = q.SortDir.Equals("asc", StringComparison.OrdinalIgnoreCase);

        query = (q.SortBy?.ToLower()) switch
        {
            "datefin"     => dirAsc ? query.OrderBy(d => d.DateFin)     : query.OrderByDescending(d => d.DateFin),
            "status"      => dirAsc ? query.OrderBy(d => d.Status)      : query.OrderByDescending(d => d.Status),
            "nombrejour"  => dirAsc ? query.OrderBy(d => d.NombreJour)  : query.OrderByDescending(d => d.NombreJour),
            "iddmd"       => dirAsc ? query.OrderBy(d => d.IdDmd)       : query.OrderByDescending(d => d.IdDmd),
            _             => dirAsc ? query.OrderBy(d => d.DateDebut)   : query.OrderByDescending(d => d.DateDebut),
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(d => new DemandeCongeDto
            {
                IdDmd = d.IdDmd,
                UserId = d.UserId,
                DateDebut = d.DateDebut,
                DebutApresMidi = d.DebutApresMidi,
                DateFin = d.DateFin,
                FinApresMidi = d.FinApresMidi,
                Motif = d.Motif,
                NombreJour = d.NombreJour,
                Status = d.Status,
                User = new UserMiniDto
                {
                    Id = d.User.Id,
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    Email = d.User.Email,
                    Phone = d.User.Phone,
                    HiringDate = d.User.HiringDate,
                    Role = d.User.Role
                }
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)q.PageSize);

        return new PagedResult<DemandeCongeDto>
        {
            CurrentPage = q.Page,
            PageSize = q.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items
        };
    }
}