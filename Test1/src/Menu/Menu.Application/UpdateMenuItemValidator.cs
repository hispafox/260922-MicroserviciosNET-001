using FluentValidation;

namespace Menu.Application;

public class UpdateMenuItemValidator : AbstractValidator<UpdateMenuItem>
{
    public UpdateMenuItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nombre requerido")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Precio mayor a cero")
            .LessThan(10000);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
    }
}
