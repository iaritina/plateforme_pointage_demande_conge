using Shared.models;

namespace BackOffice.DTO;

public class DemandeCongeDto
{
    public int IdDmd { get; set; }
    public int UserId { get; set; }
    public DateTime DateDebut { get; set; }
    public bool DebutApresMidi { get; set; }
    public DateTime DateFin { get; set; }
    public bool FinApresMidi { get; set; }
    public string? Motif { get; set; }
    public decimal NombreJour { get; set; }
    public StatusEnum? Status { get; set; }

    public UserMiniDto User { get; set; } = new();
}