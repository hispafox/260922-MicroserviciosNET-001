using Identity.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure.Security;

/// <summary>
/// Servicio para la generación de tokens JWT.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _cfg;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="JwtTokenService"/>.
    /// </summary>
    /// <param name="cfg">Instancia de <see cref="IConfiguration"/> para acceder a la configuración de la aplicación.</param>
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public Task<string> CreateTokenAsync(string userId, string userName, IEnumerable<string> roles, IEnumerable<string> scopes, string? audience = null, CancellationToken ct = default)
    {
        //var claims = new[]
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    }.Union(await _userManager.GetClaimsAsync(user));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> // Crear una lista de claims para el token
        {
            new(JwtRegisteredClaimNames.Sub, userId),  // Identificador del sujeto (usuario)
            new(JwtRegisteredClaimNames.UniqueName, userName), // Nombre único del usuario
            new("scope", string.Join(' ', scopes)) // Scopes autorizados, unidos en una sola cadena separada por espacios
        };


        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

     
        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    /// <summary>
    /// Crea un token JWT firmado para un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="userName">Nombre de usuario.</param>
    /// <param name="roles">Colección de roles asignados al usuario.</param>
    /// <param name="scopes">Colección de scopes autorizados.</param>
    /// <param name="audience">Audiencia del token (opcional).</param>
    /// <param name="ct">Token de cancelación (opcional).</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado contiene el token JWT generado como cadena.</returns>
    public Task<string> CreateTokenAsync1(
        string userId,
        string userName,
        IEnumerable<string> roles,
        IEnumerable<string> scopes,
        string? audience = null,
        CancellationToken ct = default)
    {
        var authority = _cfg["Auth:Authority"];
        var signingKey = _cfg["Auth:SigningKey"];

        //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Auth:SigningKey"]!)); // Crear una clave simétrica para firmar el token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Auth:SigningKey"])); // Crear una clave simétrica para firmar el token
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Crear las credenciales de firma utilizando HMAC SHA256
        var claims = new List<Claim> // Crear una lista de claims para el token
        {
            new(JwtRegisteredClaimNames.Sub, userId),  // Identificador del sujeto (usuario)
            new(JwtRegisteredClaimNames.UniqueName, userName), // Nombre único del usuario
            new("scope", string.Join(' ', scopes)) // Scopes autorizados, unidos en una sola cadena separada por espacios
        };
        if (!string.IsNullOrWhiteSpace(audience)) claims.Add(new Claim("aud", audience!)); // Agregar la audiencia si se proporciona
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r)); // Agregar un claim por cada rol del usuario
        var token = new JwtSecurityToken( // Crear el token JWT
            issuer: _cfg["Auth:Authority"], // Emisor del token
            claims: claims,   // Claims del token
            expires: DateTime.UtcNow.AddHours(2), // Fecha de expiración del token (2 horas desde ahora)
            signingCredentials: creds); // Credenciales de firma
        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token)); // Devolver el token como una cadena
    }
}

