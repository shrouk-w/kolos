using kolos.DTOs;

namespace kolos.Services;

public interface IVisitsService
{
    public Task<VisitResponseDTO> GetVisitsAsync(int id, CancellationToken cancellationToken);
}