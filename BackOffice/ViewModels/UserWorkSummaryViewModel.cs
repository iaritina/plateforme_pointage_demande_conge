namespace BackOffice.ViewModels;

public class UserWorkSummaryViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public double WorkMinutes { get; set; }           
    public double BreakMinutes { get; set; }          
    public double TotalWorkMinutes { get; set; }     
    public double TotalBreakMinutes { get; set; }     
    public double TotalMinutes { get; set; }         
    public double WorkPercentage { get; set; }        
}
