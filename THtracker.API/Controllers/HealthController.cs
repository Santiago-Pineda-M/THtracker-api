using Microsoft.AspNetCore.Mvc;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "THtracker API",
            timestamp = DateTime.UtcNow
        });
    }
}
