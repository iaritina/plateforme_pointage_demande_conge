using BackOffice.Models;

namespace BackOffice.ViewModels;

public class UserStatusViewModel
{
    public string LastName { get; set; } = "";
    public DateTime? LastRegistrationTime { get; set; }
    public RegistrationType LastStatus { get; set; } = RegistrationType.Exit;
    
    public string StatusText => LastStatus == RegistrationType.Enter ? "Présent" : "Absent";
    public string StatusBadgeClass => LastStatus == RegistrationType.Enter ? "bg-success" : "bg-danger";
}