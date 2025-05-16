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
    public async Task<IActionResult> GetVisitsViaId(int id, CancellationToken cancellationToken)
    {
        var visits = await _visitService.GetVisitsAsync(id, cancellationToken);
        return Ok(visits);
    }
}