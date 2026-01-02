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
                
                foreach (var s in daySchedules)
                {
                    var minutes = (TimeSpan.Parse(s.End) - TimeSpan.Parse(s.Start)).TotalMinutes;

                    if (s.Working)
                        totalWorkPlanned += minutes;
                    else if (day >= 1 && day <= 5)
                        totalBreakPlanned += minutes;
                }
                
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
                        workMinutes += duration;
                    }
                    else if (current.Status == RegistrationType.Exit &&
                             next.Status == RegistrationType.Enter)
                    {
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


    public async Task<WorkSummaryViewModel?> GetUserWorkSummary(int userId, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        DateTime today = DateTime.Today;
        DateTime start = startDate ?? today.AddDays(-2);
        DateTime end = endDate ?? today;
        
        var schedules = await _context.Schedules
            .Where(s => s.UserId == user.Id)
            .ToListAsync();
        
        var registrations = await _context.Registrations
            .Where(r => r.UserId == userId && r.Timestamp.Date >= start && r.Timestamp.Date <= end)
            .OrderBy(r => r.Timestamp)
            .ToListAsync();
        
        double totalPlannedWork = 0;
        double totalPlannedBreak = 0;
        double actualWork = 0;
        double actualBreak = 0;
        
        var scheduleVM = new List<UserScheduleViewModel>();
        var registrationVM = new List<UserRegistrationsViewModel>();
        
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            int dayNum = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

            // 🔹 Planning
            var daySchedules = schedules.Where(s => s.Day == dayNum).ToList();
            double dayPlannedWork = 0;
            double dayPlannedBreak = 0;

            var timeSlotsVM = new List<TimeSlotViewModel>();
            foreach (var s in daySchedules)
            {
                var minutes = (TimeSpan.Parse(s.End) - TimeSpan.Parse(s.Start)).TotalMinutes;
                if (s.Working) dayPlannedWork += minutes;
                else dayPlannedBreak += minutes;

                timeSlotsVM.Add(new TimeSlotViewModel
                {
                    Start = s.Start,
                    End = s.End,
                    Working = s.Working
                });
            }

            totalPlannedWork += dayPlannedWork;
            totalPlannedBreak += dayPlannedBreak;

            scheduleVM.Add(new UserScheduleViewModel
            {
                Day = dayNum,
                DayLabel = GetDayLabel(dayNum),
                TimeSlots = timeSlotsVM
            });

            // 🔹 Registrations
            var dayRegistrations = registrations
                .Where(r => r.Timestamp.Date == date)
                .ToList();

            double dayActualWork = 0;
            double dayActualBreak = 0;
            var regItems = new List<RegistrationItemViewModel>();

            for (int i = 0; i < dayRegistrations.Count - 1; i++)
            {
                var current = dayRegistrations[i];
                var next = dayRegistrations[i + 1];

                var duration = (next.Timestamp - current.Timestamp).TotalMinutes;

                if (current.Status == RegistrationType.Enter && next.Status == RegistrationType.Exit)
                    dayActualWork += duration;
                else if (current.Status == RegistrationType.Exit && next.Status == RegistrationType.Enter)
                    dayActualBreak += duration;
            }

            actualWork += dayActualWork;
            actualBreak += dayActualBreak;

            foreach (var r in dayRegistrations)
            {
                var localTimestamp = r.Timestamp.ToLocalTime();
                regItems.Add(new RegistrationItemViewModel
                {
                    Timestamp = localTimestamp,
                    Status = r.Status.ToString()
                });
            }

            registrationVM.Add(new UserRegistrationsViewModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Day = dayNum,
                DayLabel = GetDayLabel(dayNum),
                Registrations = regItems
            });
        }
        
        double efficiency = totalPlannedWork > 0 ? (actualWork / totalPlannedWork) * 100 : 0;

        return new WorkSummaryViewModel
        {
            UserId = user.Id,
            FullName = user.GetFullName(),
            StartDate = start.ToString("yyyy-MM-dd"),
            EndDate = end.ToString("yyyy-MM-dd"),
            DaysCount = (end - start).Days + 1,
            PlannedWorkMinutes = totalPlannedWork,
            PlannedBreakMinutes = totalPlannedBreak,
            ActualWorkMinutes = actualWork,
            ActualBreakMinutes = actualBreak,
            EfficiencyPercentage = Math.Round(efficiency, 2),
            Schedules = scheduleVM,
            Registrations = registrationVM
        };
    }
    
    
    private string GetDayLabel(int day)
    {
        return day switch
        {
            1 => "Lundi",
            2 => "Mardi",
            3 => "Mercredi",
            4 => "Jeudi",
            5 => "Vendredi",
            6 => "Samedi",
            7 => "Dimanche",
            _ => ""
        };
    }
    
    
}
