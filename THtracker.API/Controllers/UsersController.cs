using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THtracker.Application.DTOs;
using THtracker.Infrastructure.Persistence;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _context
            .Users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
            })
            .ToListAsync();

        return Ok(users);
    }
}
