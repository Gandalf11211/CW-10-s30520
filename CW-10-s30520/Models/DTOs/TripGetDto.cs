namespace CW_10_s30520.Models.DTOs;

public class TripGetDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryGetDto> Countries { get; set; }
    public List<ClientGetDto> Clients { get; set; }
}