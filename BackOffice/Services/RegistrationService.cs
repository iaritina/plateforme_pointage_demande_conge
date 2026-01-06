using BackOffice.Context;
using BackOffice.Hubs;
using BackOffice.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.models;

namespace BackOffice.Services;

public class RegistrationService
{
    private readonly MyDbContext _context;
    private readonly IHubContext<MonitoringHubs> _hubContext;

    public RegistrationService(MyDbContext context,  IHubContext<MonitoringHubs> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
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

        await _hubContext.Clients.All.SendAsync(
            "MonitoringUpdated",
            new
            {
                UserId = userId,
                Status = status,
                Timestamp = registration.Timestamp
            }
        );

    }
}