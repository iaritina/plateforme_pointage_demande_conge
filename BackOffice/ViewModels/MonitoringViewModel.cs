namespace BackOffice.ViewModels;

public class MonitoringViewModel
{
    public List<UserStatusViewModel> Users { get; set; } = new List<UserStatusViewModel>();

    public int TotalUsers { get; set; }
    public int PresentUsers { get; set; }
    public int AbsentUsers { get; set; }
}