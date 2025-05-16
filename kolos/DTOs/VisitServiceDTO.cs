using System.ComponentModel.DataAnnotations;

namespace kolos.DTOs;

public class VisitServiceDTO
{
    [MaxLength(100)]
    public String name{get;set;}
    public float serviceFee{get;set;}
}