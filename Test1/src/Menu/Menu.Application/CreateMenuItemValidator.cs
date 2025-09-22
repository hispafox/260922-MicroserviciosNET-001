using FluentValidation;
using Menu.Domain.ValueObjects;

namespace Menu.Application;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItem>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nombre requerido")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Precio requerido")
            .Must(p => p.Value > 0).WithMessage("Precio mayor a cero")
            .Must(p => p.Value < 10000).WithMessage("Precio menor a 10000");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
    }
}
