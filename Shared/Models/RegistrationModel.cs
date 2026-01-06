using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.models;

public class Registration
{
    public int Id { get; set; }
        
    [Required]
    public int UserId { get; set; }
        
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
        
    [Required]
    public RegistrationType Status { get; set; }

    [Required] 
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}