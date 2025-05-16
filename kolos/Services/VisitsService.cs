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

    public async Task<int> CreateNewServiceAsync(ServiceCreateRequestDTO service, CancellationToken cancellationToken)
    {
        if(await _visitisRepository.DoesVisitExist(service.visitId, cancellationToken))
            throw new ConflictException("Visit already exists");
        
        if(! await _visitisRepository.DoesClientExist(service.clientId, cancellationToken))
            throw new ConflictException("Visit already exists");

        var mechanicId = await _visitisRepository.GetMechnicId(service.mechnicLicenceNumber, cancellationToken);
        
        List<int> servicesIds = new List<int>();
        
        foreach (var visitServiceDto in service.services)
        {
            var id =await  _visitisRepository.GetServiceId(visitServiceDto.name, cancellationToken);
            servicesIds.Add(id);
        }
        
        AddServiceToDB_DTO serviceToDbDto = new AddServiceToDB_DTO()
        {
            visitId = service.visitId,
            services = servicesIds,
            clientId = service.clientId,
            mechnicId = mechanicId,
            date = DateTime.Now,
            visitServices = service.services
        };
        
        var response = await _visitisRepository.CreateNewServiceAsync(serviceToDbDto, cancellationToken);
        
        return response;
    }
}