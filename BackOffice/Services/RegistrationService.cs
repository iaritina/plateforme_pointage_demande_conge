using BackOffice.Context;
using BackOffice.Models;
using Microsoft.EntityFrameworkCore;
using Shared.models;

namespace BackOffice.Services;

public class RegistrationService
{
    private readonly MyDbContext _context;

    public RegistrationService(MyDbContext context)
    {
        _context = context;
    }

    public async Task Create(int userId, RegistrationType status)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!exists) throw new Exception("Utilisateur introuvable");
        
        var registration = new Registration
        {
            UserId = userId,
            Status = status,
            Timestamp = DateTime.UtcNow
        };
        
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();
        
    }
}