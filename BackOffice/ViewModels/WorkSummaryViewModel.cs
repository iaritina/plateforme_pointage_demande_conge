namespace BackOffice.ViewModels;

public class WorkSummaryViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;

    // Plage de dates
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int DaysCount { get; set; }

    // Résumé
    public double PlannedWorkMinutes { get; set; }
    public double PlannedBreakMinutes { get; set; }
    public double ActualWorkMinutes { get; set; }
    public double ActualBreakMinutes { get; set; }
    public double EfficiencyPercentage { get; set; }

    // Schedule
    public List<UserScheduleViewModel> Schedules { get; set; } = new List<UserScheduleViewModel>();

    // Registrations
    public List<UserRegistrationsViewModel> Registrations { get; set; } = new List<UserRegistrationsViewModel>();
}

public class UserScheduleViewModel
{
    public int Day { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public List<TimeSlotViewModel> TimeSlots { get; set; } = new List<TimeSlotViewModel>();
}

public class TimeSlotViewModel
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public bool Working { get; set; }
}

public class UserRegistrationsViewModel
{
    public string Date { get; set; } = string.Empty;
    public int Day { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public List<RegistrationItemViewModel> Registrations { get; set; } = new List<RegistrationItemViewModel>();
}

public class RegistrationItemViewModel
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}