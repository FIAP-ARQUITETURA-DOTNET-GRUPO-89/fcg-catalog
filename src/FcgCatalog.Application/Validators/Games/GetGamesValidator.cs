using FluentValidation;
using FcgCatalog.Application.Queries.Games;

namespace FcgCatalog.Application.Validators.Games;

public class GetGamesValidator : AbstractValidator<GetGamesQuery>
{
    public GetGamesValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("O tamanho da página deve ser maior que 0.")
            .LessThanOrEqualTo(50).WithMessage("O tamanho máximo da página permitido é 50.");
    }
}
