using System.ComponentModel.DataAnnotations;

namespace FrontOffice.ViewModel;

public class DemandeCongeCreateViewModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime DateDebut { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateFin { get; set; }

    [Required]
    public string Motif { get; set; }
}
