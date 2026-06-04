namespace AuthService.Services;
using AuthService.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public interface IJwtService
{
    string GenerateToken(UserMaster user);
}
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GenerateToken(UserMaster user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = jwtSettings.GetValue<int>("ExpiresMinutes");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new Exception("JWT Secret is missing");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier,user.login_id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub,user.login_id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email,user.email.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,user.email)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}