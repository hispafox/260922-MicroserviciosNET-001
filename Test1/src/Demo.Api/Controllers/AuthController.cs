using Asp.Versioning;
using Identity.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly ITokenService _tokens;
    public AuthController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, ITokenService tokens)
    { _users = users; _signIn = signIn; _tokens = tokens; }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUser cmd)
    {
        var u = new IdentityUser { UserName = cmd.UserName, Email = cmd.Email };
        var r = await _users.CreateAsync(u, cmd.Password);
        if (!r.Succeeded) return BadRequest(r.Errors);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUser cmd)
    {
        var u = await _users.FindByNameAsync(cmd.UserName);
        if (u is null) return Unauthorized();
        var ok = await _signIn.CheckPasswordSignInAsync(u, cmd.Password, false);
        if (!ok.Succeeded) return Unauthorized();
        var roles = await _users.GetRolesAsync(u);
        var token = await _tokens.CreateTokenAsync(u.Id, u.UserName!, roles, cmd.Scopes, cmd.Audience);
        return Ok(new { access_token = token });
    }
}
public record RegisterUser(string UserName, string Email, string Password);
public record LoginUser(string UserName, string Password, string? Audience, IEnumerable<string> Scopes);
