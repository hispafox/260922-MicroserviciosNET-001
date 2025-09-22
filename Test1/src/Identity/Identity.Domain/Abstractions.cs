namespace Identity.Domain.Abstractions;
public interface ITokenService
{
    Task<string> CreateTokenAsync(string userId, string userName, IEnumerable<string> roles, IEnumerable<string> scopes, string? audience = null, CancellationToken ct = default);
}
