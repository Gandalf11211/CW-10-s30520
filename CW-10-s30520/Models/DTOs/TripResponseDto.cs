namespace CW_10_s30520.Models.DTOs;

public class TripResponseDto
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public List<TripGetDto> Trips { get; set; }
}