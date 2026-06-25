using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly DmsDbContext _context;

    public UsersController(DmsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized(new { message = "Nie udało się odczytać użytkownika z tokenu." });
        }

        var users = await _context.Users
            .Where(u => u.UUID != currentUserId)
            .Select(u => new
            {
                userId = u.UUID,
                username = u.Username,
                email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }
}