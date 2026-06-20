using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using JudicialEvidence.Api.Models.Entities;

namespace JudicialEvidence.Api.Auth;

public interface IJwtTokenService
{
    string IssueToken(User user);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string IssueToken(User user)
    {
        var key = _config["Jwt:Key"] ?? "judicial-evidence-default-secret-key-2026-min32chars!!";
        var issuer = _config["Jwt:Issuer"] ?? "JudicialEvidence";
        var audience = _config["Jwt:Audience"] ?? "JudicialEvidenceClient";
        var expires = DateTime.UtcNow.AddHours(
            double.TryParse(_config["Jwt:ExpiresHours"], out var h) ? h : 8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("fullname", user.FullName)
        };

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
