using System.ComponentModel.DataAnnotations;

namespace kolos.DTOs;

public class ServiceCreateRequestDTO
{   
    [Required]
    public int visitId { get; set; }
    [Required]
    public int clientId { get; set; }
    [Required]
    public string mechnicLicenceNumber { get; set; }
    [Required]
    public List<VisitServiceDTO> services { get; set; }
}