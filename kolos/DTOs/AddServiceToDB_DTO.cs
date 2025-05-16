namespace kolos.DTOs;

public class AddServiceToDB_DTO
{
    public int visitId { get; set; }
    public int clientId { get; set; }
    public int mechnicId { get; set; }
    public List<int> services { get; set; }
    
    public List<VisitServiceDTO> visitServices { get; set; }
    public DateTime date { get; set; }
}