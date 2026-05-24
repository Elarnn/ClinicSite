using Microsoft.AspNetCore.Mvc;

namespace CliniqueSite.Api.Controllers;

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
            message = "CliniqueSite API is running",
            timestamp = DateTime.UtcNow
        });
    }
}