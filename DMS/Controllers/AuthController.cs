using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DmsDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(DmsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usernameTaken = await _context.Users
            .AnyAsync(u => u.Username == request.Username);

        if (usernameTaken)
        {
            return BadRequest(new { message = "Nazwa użytkownika jest już zajęta." });
        }

        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailTaken)
        {
            return BadRequest(new { message = "Email jest już zajęty." });
        }

        var user = new User
        {
            UUID = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email
        };

        user.Password = _passwordHasher.HashPassword(user, request.Password);

        var userInfo = new UserInfo
        {
            UUID = user.UUID,
            Language = request.Language,
            DisplayMode = "ciemny"
        };

        user.Info = userInfo;

        _context.Users.Add(user);
        _context.UserInfos.Add(userInfo);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Konto zostało utworzone.",
            userId = user.UUID,
            username = user.Username,
            email = user.Email
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == request.UsernameOrEmail ||
                u.Email == request.UsernameOrEmail
            );

        if (user == null)
        {
            return Unauthorized(new { message = "Nieprawidłowy login/email lub hasło." });
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.Password,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Nieprawidłowy login/email lub hasło." });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token = token,
            userId = user.UUID,
            username = user.Username,
            email = user.Email
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Info)
            .FirstOrDefaultAsync(u => u.UUID == userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            userId = user.UUID,
            username = user.Username,
            email = user.Email,
            language = user.Info != null ? user.Info.Language : "pl"
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey =
            _configuration["Jwt:Key"] ??
            Environment.GetEnvironmentVariable("JWT_KEY") ??
            "super_secret_jwt_signing_key_for_dev_only";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UUID.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}