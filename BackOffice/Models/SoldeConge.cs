using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.models;

namespace BackOffice.Models;

public class SoldeConge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdConge { get; set; }

    public int IdEmploye { get; set; }
    [ForeignKey("IdEmploye")] public User Employe { get; set; } = null!;

    public decimal SoldeTotal { get; set; } = 30;
    public decimal SoldeRestant { get; set; } = 30;
}