using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Menu.Domain.Abstractions;
using Menu.Application;
using Asp.Versioning;
using Demo.Api; // Import ApiResponse
using Microsoft.AspNetCore.JsonPatch;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/menu")]
[ApiVersion("1.0")]
public class MenuController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly MenuService _menuService;
    public MenuController(IUnitOfWork unitOfWork, MenuService menuService)
    {
        _unitOfWork = unitOfWork;
        _menuService = menuService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<MenuItemDto>>>> GetMenuItems(int page = 1, int size = 20, CancellationToken cancellationToken = default)
    {
        var menuItemsQuery = _unitOfWork.MenuItemRepository.Query()
            .OrderBy(menuItem => menuItem.Name)
            .Skip((page - 1) * size)
            .Take(size);

        var menuItems = await menuItemsQuery
            .Select(menuItem => new MenuItemDto(menuItem.Id, menuItem.Name, menuItem.Price, menuItem.Stock))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IEnumerable<MenuItemDto>>.SuccessResult(menuItems));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> GetMenuItemById(int id, CancellationToken cancellationToken)
    {
        var menuItem = await _unitOfWork.MenuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem is null)
            return NotFound(ApiResponse<MenuItemDto>.ErrorResult($"Menu item with id {id} not found."));

        var menuItemDto = new MenuItemDto(menuItem.Id, menuItem.Name, menuItem.Price, menuItem.Stock);
        return Ok(ApiResponse<MenuItemDto>.SuccessResult(menuItemDto));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> CreateMenuItem(CreateMenuItem createMenuItemCommand, CancellationToken cancellationToken)
    {
        var newMenuItemId = await _menuService.CreateAsync(createMenuItemCommand, cancellationToken);
        return CreatedAtAction(nameof(GetMenuItemById), new { id = newMenuItemId, version = "1.0" }, ApiResponse<object>.SuccessResult(null, "Menu item created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> UpdateMenuItem(int id, UpdateMenuItem updateMenuItemCommand, CancellationToken cancellationToken)
    {
        await _menuService.UpdateAsync(id, updateMenuItemCommand, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(null, "Menu item updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMenuItem(int id, CancellationToken cancellationToken)
    {
        var menuItem = await _unitOfWork.MenuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem is null)
            return NotFound(ApiResponse<object>.ErrorResult($"Menu item with id {id} not found."));
        _unitOfWork.MenuItemRepository.Remove(menuItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(null, "Menu item deleted successfully."));
    }

    [HttpPatch("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> PatchMenuItem(int id, [FromBody] JsonPatchDocument<Menu.Domain.Entities.MenuItem> patchDoc, CancellationToken cancellationToken)
    {
        if (patchDoc == null)
            return BadRequest(ApiResponse<object>.ErrorResult("Invalid patch document."));

        var menuItem = await _unitOfWork.MenuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem is null)
            return NotFound(ApiResponse<object>.ErrorResult($"Menu item with id {id} not found."));

        patchDoc.ApplyTo(menuItem, (error) => ModelState.AddModelError("patch", error.ErrorMessage));
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResult("Invalid patch operation."));

        menuItem.UpdatedAt = DateTimeOffset.UtcNow;
        _unitOfWork.MenuItemRepository.Update(menuItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(null, "Menu item patched successfully."));
    }
}

