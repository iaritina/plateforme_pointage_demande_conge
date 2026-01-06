using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.models;

namespace BackOffice.Models;

public class DemandeConge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDmd { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }

    public string Motif { get; set; }
    public decimal NombreJour { get; set; }

    public StatusEnum Status { get; set; }
}