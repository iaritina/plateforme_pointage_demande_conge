using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.models;

public class DemandeConge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDmd { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime DateDebut { get; set; }
    public bool DebutApresMidi { get; set; } // false = matin, true = après-midi
    public DateTime DateFin { get; set; }
    public bool FinApresMidi { get; set; } // false = matin, true = après-midi


    public string Motif { get; set; }
    public decimal NombreJour { get; set; }

    public int decisionYear { get; set; }
    public StatusEnum Status { get; set; }
}