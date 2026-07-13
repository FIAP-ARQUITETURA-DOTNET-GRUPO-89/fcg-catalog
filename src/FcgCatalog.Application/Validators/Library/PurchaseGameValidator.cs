using FluentValidation;
using FcgCatalog.Application.Commands.Library;

namespace FcgCatalog.Application.Validators.Library;

public class PurchaseGameValidator : AbstractValidator<PurchaseGameCommand>
{
    public PurchaseGameValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
    }
}
