using FluentValidation;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Domain.Enums;

namespace FcgCatalog.Application.Validators.Games;

public class UpdateGameValidator : AbstractValidator<UpdateGameCommand>
{
    private static readonly ClassificacaoEtaria[] ValoresValidos =
        Enum.GetValues<ClassificacaoEtaria>();

    public UpdateGameValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Preco)
            .GreaterThan(0)
            .LessThanOrEqualTo(9999.99m)
            .PrecisionScale(7, 2, false);

        RuleFor(x => x.DataLancamento)
            .NotEmpty();

        RuleFor(x => x.ClassificacaoEtaria)
            .Must(valor => ValoresValidos.Contains(valor))
                .WithMessage($"Classificação etária inválida. Valores aceitos: {string.Join(", ", ValoresValidos.Select(v => $"{(int)v} ({v})"))}.");
    }
}
