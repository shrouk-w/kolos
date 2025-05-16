using kolos.DTOs;
using kolos.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolos.Controllers;


[ApiController]
[Route("api/[controller]")]
public class VisitsController:ControllerBase
{
    private readonly IVisitsService _visitService;

    public VisitsController(IVisitsService visitService)
    {
        _visitService = visitService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVisitsViaIdAsync(int id, CancellationToken cancellationToken)
    {
        var visits = await _visitService.GetVisitsAsync(id, cancellationToken);
        return Ok(visits);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewServiceAsync(ServiceCreateRequestDTO service, CancellationToken cancellationToken)
    {
        var createdId = await _visitService.CreateNewServiceAsync(service, cancellationToken);
        
        return Created("/api/visits/", new { id = createdId });
    }
    
}