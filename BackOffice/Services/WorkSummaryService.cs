using BackOffice.Context;
using BackOffice.Models;
using BackOffice.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Services;

public class WorkSummaryService
{
    private readonly MyDbContext _context;

    public WorkSummaryService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserWorkSummaryViewModel>> GetWorkSummary(DateTime startDate, DateTime endDate)
    {
        var users = await _context.Users.ToListAsync();
        var schedules = await _context.Schedules.ToListAsync();
        var registrations = await _context.Registrations.ToListAsync();

        var results = new List<UserWorkSummaryViewModel>();

        foreach (var user in users)
        {
            double workMinutes = 0;
            double breakMinutes = 0;
            double totalWorkPlanned = 0;
            double totalBreakPlanned = 0;

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                int day = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

                var daySchedules = schedules
                    .Where(s => s.UserId == user.Id && s.Day == day)
                    .ToList();

                // 🔹 Planning (référence)
                foreach (var s in daySchedules)
                {
                    var minutes = (TimeSpan.Parse(s.End) - TimeSpan.Parse(s.Start)).TotalMinutes;

                    if (s.Working)
                        totalWorkPlanned += minutes;
                    else if (day >= 1 && day <= 5)
                        totalBreakPlanned += minutes;
                }

                // 🔹 Pointages (réel)
                var dayRegistrations = registrations
                    .Where(r => r.UserId == user.Id &&
                                r.Timestamp.ToLocalTime().Date == date)
                    .OrderBy(r => r.Timestamp)
                    .ToList();

                for (int i = 0; i < dayRegistrations.Count - 1; i++)
                {
                    var current = dayRegistrations[i];
                    var next = dayRegistrations[i + 1];

                    var duration = (next.Timestamp.ToLocalTime() - current.Timestamp.ToLocalTime()).TotalMinutes;

                    if (current.Status == RegistrationType.Enter &&
                        next.Status == RegistrationType.Exit)
                    {
                        // Travail réel
                        workMinutes += duration;
                    }
                    else if (current.Status == RegistrationType.Exit &&
                             next.Status == RegistrationType.Enter)
                    {
                        // Pause réelle
                        breakMinutes += duration;
                    }
                }
            }

            double workPercentage = totalWorkPlanned > 0
                ? (workMinutes / totalWorkPlanned) * 100
                : 0;

            results.Add(new UserWorkSummaryViewModel
            {
                UserId = user.Id,
                FullName = user.GetFullName(),
                WorkMinutes = Math.Round(workMinutes, 2),
                BreakMinutes = Math.Round(breakMinutes, 2),
                TotalWorkMinutes = totalWorkPlanned,
                TotalBreakMinutes = totalBreakPlanned,
                TotalMinutes = totalWorkPlanned + totalBreakPlanned,
                WorkPercentage = Math.Round(workPercentage, 2)
            });
        }

        return results;
    }
}
