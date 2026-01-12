using BackOffice.ViewModels;
using Microsoft.EntityFrameworkCore;
using Shared.Context;
using Shared.models;

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
        var leaves = await _context.DemandeConges
            .Where(c => c.Status == StatusEnum.ok &&
                        c.DateFin.Date >= startDate &&
                        c.DateDebut.Date <= endDate)
            .ToListAsync();

        var results = new List<UserWorkSummaryViewModel>();

        foreach (var user in users)
        {
            double workMinutes = 0;
            double breakMinutes = 0;
            double totalWorkPlanned = 0;
            double totalBreakPlanned = 0;

            // Créneaux de congés par utilisateur
            var leaveSlots = BuildLeaveSlots(leaves.Where(l => l.UserId == user.Id).ToList(), startDate, endDate);

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                int day = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

                var daySchedules = schedules
                    .Where(s => s.UserId == user.Id && s.Day == day)
                    .ToList();

                var leave = leaveSlots.FirstOrDefault(l => l.Date == date.ToString("yyyy-MM-dd"));

                foreach (var s in daySchedules)
                {
                    var slotStartMinutes = TimeSpan.Parse(s.Start).TotalMinutes;
                    var slotEndMinutes = TimeSpan.Parse(s.End).TotalMinutes;
                    var slotDuration = slotEndMinutes - slotStartMinutes;

                    double work = s.Working ? slotDuration : 0;
                    double pause = s.Working ? 0 : slotDuration;

                    if (leave != null)
                    {
                        if (s.Working)
                        {
                            // Réduire le travail si congé
                            double morningReduction = leave.Morning && slotStartMinutes < 12 * 60
                                ? Math.Min(slotEndMinutes, 12 * 60) - slotStartMinutes
                                : 0;
                            double afternoonReduction = leave.Afternoon && slotEndMinutes > 12 * 60
                                ? slotEndMinutes - Math.Max(slotStartMinutes, 12 * 60)
                                : 0;

                            work -= morningReduction + afternoonReduction;
                            if (work < 0) work = 0;
                        }
                        else
                        {
                            // Pause annulée si congé
                            pause = 0;
                        }
                    }
                    else if (!s.Working && slotStartMinutes == 0 && (slotEndMinutes == 1439 || slotEndMinutes == 1440))
                    {
                        // Jour non travaillé (week-end ou off) => pause = 0
                        pause = 0;
                    }

                    if (s.Working)
                        totalWorkPlanned += work;
                    else
                        totalBreakPlanned += pause;
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
                        workMinutes += duration;
                    else if (current.Status == RegistrationType.Exit &&
                             next.Status == RegistrationType.Enter)
                        breakMinutes += duration;
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


    public async Task<WorkSummaryViewModel?> GetUserWorkSummary(
        int userId,
        DateTime? startDate = null,
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
            .Where(r =>
                r.UserId == userId &&
                r.Timestamp.Date >= start &&
                r.Timestamp.Date <= end)
            .OrderBy(r => r.Timestamp)
            .ToListAsync();

        // 🔹 Congés validés
        var leaves = await _context.DemandeConges
            .Where(c =>
                c.UserId == userId &&
                c.Status == StatusEnum.ok &&
                c.DateFin.Date >= start &&
                c.DateDebut.Date <= end)
            .ToListAsync();

        var leaveSlots = BuildLeaveSlots(leaves, start, end);

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

            var leave = leaveSlots.FirstOrDefault(l => l.Date == date.ToString("yyyy-MM-dd"));

            foreach (var s in daySchedules)
            {
                var slotStartMinutes = TimeSpan.Parse(s.Start).TotalMinutes;
                var slotEndMinutes = TimeSpan.Parse(s.End).TotalMinutes;
                var slotDuration = slotEndMinutes - slotStartMinutes;

                double workMinutes = s.Working ? slotDuration : 0;
                double breakMinutes = s.Working ? 0 : slotDuration;

                if (leave != null)
                {
                    if (s.Working)
                    {
                        // 🔹 Réduire le temps de travail en fonction du congé
                        double morningReduction = leave.Morning && slotStartMinutes < 12 * 60
                            ? Math.Min(slotEndMinutes, 12 * 60) - slotStartMinutes
                            : 0;
                        double afternoonReduction = leave.Afternoon && slotEndMinutes > 12 * 60
                            ? slotEndMinutes - Math.Max(slotStartMinutes, 12 * 60)
                            : 0;

                        workMinutes -= morningReduction + afternoonReduction;
                        if (workMinutes < 0) workMinutes = 0;
                    }
                    else
                    {
                        // 🔹 Créneau de pause annulé si congé ou jour non travaillé
                        breakMinutes = 0;
                    }
                }
                else if (!s.Working && slotStartMinutes == 0 && (slotEndMinutes == 1439 || slotEndMinutes == 1440))
                {
                    // 🔹 Jour non travaillé (week-end ou off) => pause = 0
                    breakMinutes = 0;
                }

                dayPlannedWork += workMinutes;
                dayPlannedBreak += breakMinutes;

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

            // 🔹 Pointages
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

                if (current.Status == RegistrationType.Enter &&
                    next.Status == RegistrationType.Exit)
                    dayActualWork += duration;
                else if (current.Status == RegistrationType.Exit &&
                         next.Status == RegistrationType.Enter)
                    dayActualBreak += duration;
            }

            actualWork += dayActualWork;
            actualBreak += dayActualBreak;

            foreach (var r in dayRegistrations)
            {
                regItems.Add(new RegistrationItemViewModel
                {
                    Timestamp = r.Timestamp.ToLocalTime(),
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

        double efficiency = totalPlannedWork > 0
            ? (actualWork / totalPlannedWork) * 100
            : 0;

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
            Registrations = registrationVM,
            Leaves = leaveSlots
        };
    }
    
    private List<LeaveSlotViewModel> BuildLeaveSlots(
        List<DemandeConge> leaves,
        DateTime start,
        DateTime end)
    {
        var result = new Dictionary<DateTime, LeaveSlotViewModel>();

        foreach (var leave in leaves)
        {
            for (var d = leave.DateDebut.Date; d <= leave.DateFin.Date; d = d.AddDays(1))
            {
                if (d < start || d > end) continue;

                if (!result.ContainsKey(d))
                {
                    result[d] = new LeaveSlotViewModel
                    {
                        Date = d.ToString("yyyy-MM-dd"),
                        Morning = false,
                        Afternoon = false
                    };
                }

                if (d == leave.DateDebut.Date)
                {
                    if (!leave.DebutApresMidi)
                        result[d].Morning = true;

                    result[d].Afternoon = true;
                }
                else if (d == leave.DateFin.Date)
                {
                    result[d].Morning = true;

                    if (leave.FinApresMidi)
                        result[d].Afternoon = true;
                }
                else
                {
                    result[d].Morning = true;
                    result[d].Afternoon = true;
                }
            }
        }

        return result.Values.ToList();
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
