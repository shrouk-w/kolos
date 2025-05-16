using kolos.DTOs;
using kolos.Exceptions;
using kolos.Repositories;

namespace kolos.Services;

public class VisitsService : IVisitsService
{
    
    private readonly IVisitsRepository _visitisRepository;

    public VisitsService(IVisitsRepository visitisRepository)
    {
        _visitisRepository = visitisRepository;
    }
    
    public async Task<VisitResponseDTO> GetVisitsAsync(int id, CancellationToken cancellationToken)
    {
        if(id<=0)
            throw new BadRequestException("Invalid VisitId");
        var visit = await _visitisRepository.GetVisitsAsync(id, cancellationToken);



        return visit;
    }
}