using FrontOffice.Repository;
using Shared.models;

namespace FrontOffice.Service;

public class DemandeCongeService
{
    private readonly DemandeCongeRepository  _demandeCongeRepository;

    public DemandeCongeService(DemandeCongeRepository demandeCongeRepository)
    {
        _demandeCongeRepository = demandeCongeRepository;
    }
    
    public Task<List<DemandeConge>> GetUserDemandesAsync(int userId, CancellationToken ct = default)
    {
        return _demandeCongeRepository.GetByUserIdAsync(userId, ct);
    }
    
    public async Task<DemandeConge> CreerDemandeAsync(DemandeConge demande, CancellationToken ct = default)
    {
        if (demande == null) throw new ArgumentNullException(nameof(demande));

        var nbJours = CalculerJoursOuvres(demande);
        if (nbJours <= 0m)
            throw new InvalidOperationException("La demande n'a aucun jour ouvré à poser.");

        demande.NombreJour = nbJours;
        demande.Status = StatusEnum.pending;

        var solde = await _demandeCongeRepository.GetSoldeRestantAsync(
            demande.UserId,
            demande.decisionYear,
            ct);

        if (solde == null)
            throw new InvalidOperationException($"Aucun solde trouvé pour l'année {demande.decisionYear}.");

        if (solde.Value < demande.NombreJour)
            throw new InvalidOperationException(
                $"Solde insuffisant pour {demande.decisionYear} (reste {solde:0.##} j, demande {demande.NombreJour:0.##} j).");

        return await _demandeCongeRepository.CreerDemandeAsync(demande, ct);
    }
    
    
    private decimal CalculerJoursOuvres(DemandeConge d)
    {
        decimal total = 0;

        // Parcourir de la date de départ à la date de retour
        for (var date = d.DateDebut.Date; date <= d.DateFin.Date; date = date.AddDays(1))
        {
            // Ignorer weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // CAS 1 : Même jour (départ et retour le même jour)
            if (d.DateDebut.Date == d.DateFin.Date)
            {
                if (!d.DebutApresMidi && !d.FinApresMidi)
                    return 0m;
            
                if (!d.DebutApresMidi && d.FinApresMidi)
                    return 0.5m;
            
                if (d.DebutApresMidi && d.FinApresMidi)
                    return 0m;
            
                return 0m;
            }

            // CAS 2 : Plusieurs jours
            // Premier jour (jour de départ)
            if (date == d.DateDebut.Date)
            {
                total += d.DebutApresMidi ? 0.5m : 1m;
            }
            else if (date == d.DateFin.Date)
            {
                total += d.FinApresMidi ? 0.5m : 0m;
            }
            // Jours intermédiaires
            else
            {
                total += 1m;
            }
        }

        return total;
    }
}