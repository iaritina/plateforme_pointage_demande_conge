namespace BackOffice.ViewModels;

public class UserWorkSummaryViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public double WorkMinutes { get; set; }           // travail effectif
    public double BreakMinutes { get; set; }          // pause prise
    public double TotalWorkMinutes { get; set; }      // travail planifié
    public double TotalBreakMinutes { get; set; }     // pause planifiée
    public double TotalMinutes { get; set; }          // travail + pause planifié
    public double WorkPercentage { get; set; }        // % travail effectué
}
