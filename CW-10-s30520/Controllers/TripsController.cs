using CW_10_s30520.Models.DTOs;
using CW_10_s30520.Service;
using Microsoft.AspNetCore.Mvc;

namespace CW_10_s30520.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var trips = await service.GetTripsAsync(page, pageSize);
            return Ok(trips);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip([FromBody] ClientTripGetDto clientTripGetDto)
    {
        try
        {
            await service.AssignClientToTripAsync(clientTripGetDto);
            return Ok(
                $"Client {clientTripGetDto.FirstName} {clientTripGetDto.LastName} assigned to trip with id {clientTripGetDto.IdTrip} successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}