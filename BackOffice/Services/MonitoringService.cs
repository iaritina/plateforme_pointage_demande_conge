using BackOffice.Context;
using BackOffice.Models;
using BackOffice.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Services;

public class MonitoringService
{
    private readonly MyDbContext _context;
    
    public MonitoringService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<MonitoringViewModel> GetStatusAsync()
    {
        try
        {
            var today = DateTime.Today;
            
            var users = await _context.Users
                .Include(u => u.Registrations)
                .ToListAsync();

            var userStatusList = new List<UserStatusViewModel>();

            int present = 0;
            int absent = 0;

            foreach (var user in users)
            {
                var lastRegistration = user.Registrations
                    .Where(r => r.Timestamp.ToLocalTime().Date == today)
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefault();

                var status = lastRegistration?.Status ?? RegistrationType.Exit;

                if (status == RegistrationType.Enter)
                    present++;
                else
                    absent++;

                userStatusList.Add(new UserStatusViewModel
                {
                    LastName = user.LastName,
                    LastRegistrationTime = lastRegistration?.Timestamp.ToLocalTime(),
                    LastStatus = status
                });
            }

            return new MonitoringViewModel
            {
                Users = userStatusList,
                TotalUsers = users.Count,
                PresentUsers = present,
                AbsentUsers = absent
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Erreur lors de la récupération du statut des utilisateurs", ex);
        }
    }
}
