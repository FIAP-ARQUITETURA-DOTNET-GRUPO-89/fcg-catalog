using FluentValidation;
using FcgCatalog.Application.Commands.Games;

namespace FcgCatalog.Application.Validators.Games;

public class UpdatePriceValidator : AbstractValidator<UpdatePriceCommand>
{
    public UpdatePriceValidator()
    {
        RuleFor(x => x.NovoPreco)
            .GreaterThan(0)
            .LessThanOrEqualTo(9999.99m)
            .PrecisionScale(7, 2, false);
    }
}
