using kolos.DTOs;

namespace kolos.Repositories;

public interface IVisitsRepository
{
    public Task<VisitResponseDTO> GetVisitsAsync(int id, CancellationToken cancellationToken);
}