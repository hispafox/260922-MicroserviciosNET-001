using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Menu.Domain.Abstractions;
using Menu.Application;
using Asp.Versioning;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/menu")]
[ApiVersion("1.0")]
public class MenuController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly MenuService _svc;
    public MenuController(IUnitOfWork uow, MenuService svc) { _uow = uow; _svc = svc; }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> Get(int page = 1, int size = 20, CancellationToken ct = default)
    {
        var q = _uow.MenuItems.Query().OrderBy(x => x.Name).Skip((page - 1) * size).Take(size);
        var items = await q.Select(i => new MenuItemDto(i.Id, i.Name, i.Price, i.Stock)).ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuItemDto>> GetById(int id, CancellationToken ct)
    {
        var i = await _uow.MenuItems.GetByIdAsync(id, ct);
        return i is null ? NotFound() : Ok(new MenuItemDto(i.Id, i.Name, i.Price, i.Stock));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateMenuItem cmd, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, null);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateMenuItem cmd, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, cmd, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var e = await _uow.MenuItems.GetByIdAsync(id, ct);
        if (e is null) return NotFound();
        _uow.MenuItems.Remove(e); await _uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
