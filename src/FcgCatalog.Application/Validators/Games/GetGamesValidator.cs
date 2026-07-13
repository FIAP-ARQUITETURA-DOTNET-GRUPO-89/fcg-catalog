using FluentValidation;
using FcgCatalog.Application.Queries.Games;

namespace FcgCatalog.Application.Validators.Games;

public class GetGamesValidator : AbstractValidator<GetGamesQuery>
{
    public GetGamesValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);
    }
}
