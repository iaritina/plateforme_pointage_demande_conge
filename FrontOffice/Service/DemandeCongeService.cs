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
}