using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackOffice.Models;

[Table("User")]
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; }

    [Required, MaxLength(100)]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Phone { get; set; }
        
    public string? Password { get; set; }

    [Required]
    public DateTime HiringDate { get; set; }
    
    [Required, MaxLength(50)]
    public string Role { get; set; } = "User";
        
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }
}