using FluentValidation;
using FcgCatalog.Application.Commands.Orders;
using FcgCatalog.Domain.Enums;

namespace FcgCatalog.Application.Validators.Orders;

public class ForceOrderStatusValidator : AbstractValidator<ForceOrderStatusCommand>
{
    public ForceOrderStatusValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(x => Enum.TryParse<OrderStatus>(x, true, out _))
            .WithMessage($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");
    }
}
