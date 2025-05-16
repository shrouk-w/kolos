using kolos.DTOs;

namespace kolos.Repositories;

public interface IVisitsRepository
{
    public Task<VisitResponseDTO> GetVisitsAsync(int id, CancellationToken cancellationToken);
    public Task<int> CreateNewServiceAsync(AddServiceToDB_DTO serviceToDbDto, CancellationToken cancellationToken);
    Task<bool> DoesVisitExist(int serviceVisitId, CancellationToken cancellationToken);
    Task<bool> DoesClientExist(int serviceClientId, CancellationToken cancellationToken);
    Task<int> GetMechnicId(string serviceMechnicLicenceNumber, CancellationToken cancellationToken);
    Task<int> GetServiceId(string name, CancellationToken cancellationToken);
}