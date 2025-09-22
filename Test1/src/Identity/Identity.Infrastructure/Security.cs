using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Security;
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public Task<string> CreateTokenAsync(string userId, string userName, IEnumerable<string> roles, IEnumerable<string> scopes, string? audience = null, CancellationToken ct = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Auth:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new("scope", string.Join(' ', scopes))
        };
        if (!string.IsNullOrWhiteSpace(audience)) claims.Add(new Claim("aud", audience!));
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        var token = new JwtSecurityToken(
            issuer: _cfg["Auth:Authority"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);
        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
