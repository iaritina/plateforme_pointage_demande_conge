using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.models;

public class SoldeConge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdConge { get; set; }

    public int IdEmploye { get; set; }
    [ForeignKey("IdEmploye")] public User Employe { get; set; } = null!;

    public int Year { get; set; }
    public decimal SoldeRestant { get; set; } = 30;
}