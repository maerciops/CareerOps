using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CareerOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    [HttpPost("login-dev")]
    public async Task<IActionResult> LoginDev([FromBody] string email, [FromServices] CareerOps.Infrastructure.Persistence.ApplicationDbContext db)
    {
        // 1. O PORTEIRO: Verifica se o email está na lista VIP
        var isInvited = await db.InvitedUser.AnyAsync(u => u.Email.ToLower() == email.ToLower());

        if (!isInvited)
        {
            // Se não estiver na lista 403 Forbidden
            return Forbid();
        }

        var userId = email == "maercio10@gmail.com"
            ? "3dd4204e-0398-4b00-a168-279b01eeba83"
            : Guid.NewGuid().ToString();

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Name, "User VIP")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}