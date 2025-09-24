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
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;
    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var user = new IdentityUser {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true,
            LockoutEnabled = false
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null) return Unauthorized();
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);
        var token = await _tokenService.CreateTokenAsync(user.Id, user.UserName!, roles, request.Scopes, request.Audience);
        return Ok(new { access_token = token });
    }
}

public record RegisterUserRequest(string UserName, string Email, string Password);
public record LoginUserRequest(string UserName, string Password, string? Audience, IEnumerable<string> Scopes);
