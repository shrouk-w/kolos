namespace kolos.DTOs;

public class VisitResponseDTO
{
    public DateTime date { get; set; }
    public ClientDTO client { get; set; }
    public MechanicDTO mechanic { get; set; }
    public List<VisitServiceDTO> VisitServices { get; set; }
}