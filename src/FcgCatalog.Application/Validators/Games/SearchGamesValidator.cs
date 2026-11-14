using FluentValidation;
using FcgCatalog.Application.Queries.Games;

namespace FcgCatalog.Application.Validators.Games;

public class SearchGamesValidator : AbstractValidator<SearchGamesQuery>
{
    // Limite padrão de paginação do OpenSearch/Elasticsearch (index.max_result_window).
    public const int MaxResultWindow = 10_000;

    public SearchGamesValidator()
    {
        RuleFor(x => x.Q)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .Must((query, page) => (long)page * query.PageSize <= MaxResultWindow)
            .WithMessage($"A busca permite navegar apenas pelos primeiros {MaxResultWindow} resultados.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);
    }
}
