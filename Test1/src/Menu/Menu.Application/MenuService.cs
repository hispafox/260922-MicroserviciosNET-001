using System;
using System.Linq;
using FluentValidation;
using Menu.Domain.Abstractions;
using Menu.Domain.Entities;

namespace Menu.Application;
public class MenuService
{
    private readonly IUnitOfWork _uow;
    public MenuService(IUnitOfWork uow) => _uow = uow;
    public async Task<int> CreateAsync(CreateMenuItem cmd, CancellationToken ct)
    {
        var validator = new CreateMenuItemValidator();
        var validationResult = validator.Validate(cmd);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var e = new MenuItem { Name = cmd.Name, Price = cmd.Price, Stock = cmd.Stock };
        
        await _uow.MenuItemRepository.AddAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        
        
        return e.Id;
    }
    public async Task UpdateAsync(int id, UpdateMenuItem cmd, CancellationToken ct)
    {
        var validator = new UpdateMenuItemValidator();
        var validationResult = validator.Validate(cmd);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var e = await _uow.MenuItemRepository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
        e.Name = cmd.Name; e.Price = cmd.Price; e.Stock = cmd.Stock; e.UpdatedAt = DateTimeOffset.UtcNow;
        _uow.MenuItemRepository.Update(e); await _uow.SaveChangesAsync(ct);
    }
}
