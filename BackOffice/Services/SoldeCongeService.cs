using BackOffice.Context;
using BackOffice.Models;
using Microsoft.EntityFrameworkCore;
using Shared.models;

namespace BackOffice.Services;

public class SoldeCongeService
{
    private readonly MyDbContext _context;

    public SoldeCongeService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<(List<SoldeConge> Items, int TotalCount)>
        GetAllPagedAsync(int page, int pageSize)
    {
        var query = _context.SoldeConges
            .Include(s => s.Employe)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.IdEmploye)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<User>> FetchEmployeeWhoDoNotHaveSolde()
    {
        return await _context.Users
            .Where(u => !_context.SoldeConges
                .Any(sc => sc.IdEmploye == u.Id))
            .ToListAsync();
    }

    public async Task<SoldeConge?> GetByEmployeeAsync(int employeeId)
    {
        return await _context.SoldeConges
            .FirstOrDefaultAsync(s => s.IdEmploye == employeeId);
    }

    public async Task CreateAsync(int employeeId, decimal soldeInitial = 30)
    {
        var solde = new SoldeConge
        {
            IdEmploye = employeeId,
            SoldeRestant = soldeInitial
        };

        _context.SoldeConges.Add(solde);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSoldeAsync(int id, decimal nouveauSolde)
    {
        var solde = await _context.SoldeConges.FindAsync(id);
        if (solde == null)
            throw new Exception("Solde introuvable");

        solde.SoldeRestant = nouveauSolde;
        await _context.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        var solde = await _context.SoldeConges.FindAsync(id);
        if (solde == null)
            return;

        _context.SoldeConges.Remove(solde);
        await _context.SaveChangesAsync();
    }
}