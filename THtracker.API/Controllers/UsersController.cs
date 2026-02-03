using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var users = new List<UserDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@thtracker.com"
            }
        };

        return Ok(users);
    }
}