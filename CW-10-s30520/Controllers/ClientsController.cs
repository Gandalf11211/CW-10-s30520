using CW_10_s30520.Exceptions;
using CW_10_s30520.Service;
using Microsoft.AspNetCore.Mvc;

namespace CW_10_s30520.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IDbService service) :  ControllerBase
{
    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteClient(int clientId)
    {
        try
        {
            await service.DeleteClientByIdAsync(clientId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
