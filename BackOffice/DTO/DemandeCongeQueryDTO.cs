using Shared.models;

namespace BackOffice.DTO;

public class DemandeCongeQueryDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int? UserId { get; set; }
    public StatusEnum? Status { get; set; }       
    public DateTime? DateFrom { get; set; }   
    public DateTime? DateTo { get; set; }
    
    public string? FullName { get; set; }   // ✅ nouveau filtre
    
    public string? Motif { get; set; }

    public string? SortBy { get; set; } = "DateDebut"; 
    public string? SortDir { get; set; } = "desc";     
}