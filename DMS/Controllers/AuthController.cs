using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DmsDbContext _context;

    public AuthController(DmsDbContext context) => _context = context;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var cleanEmail = request.Email.Trim().ToLower();
        var cleanUsername = request.Username.Trim();

        if (await _context.Users.AnyAsync(u => u.Email == cleanEmail || u.Username == cleanUsername))
            return BadRequest("Użytkownik o takim loginie lub emailu już istnieje.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                UUID = Guid.NewGuid(),
                Username = cleanUsername,
                Email = cleanEmail,
                Password = passwordHash
            };

            var userInfo = new UserInfo
            {
                UUID = user.UUID,
                Language = request.Language,
                DisplayMode = "jasny"
            };

            _context.Users.Add(user);
            _context.UserInfos.Add(userInfo);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Konto utworzone pomyślnie." });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Błąd serwera podczas rejestracji.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Unauthorized("Nieprawidłowy login lub hasło.");
        }

        // Tutaj wygenerujemy token JWT
        return Ok(new { Token = "Tu_Bedzie_Twoj_JWT_Token", UserId = user.UUID });
    }
}