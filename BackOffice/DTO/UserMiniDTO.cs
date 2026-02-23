namespace BackOffice.DTO;

public class UserMiniDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime HiringDate { get; set; }
    public string? Role { get; set; }
}