using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "OK",
            message = "ClinicSite API is running",
            timestamp = DateTime.UtcNow
        });
    }
}