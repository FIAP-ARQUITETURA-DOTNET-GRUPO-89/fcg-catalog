using FluentValidation;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;

namespace FcgCatalog.Application.Validators.Games;

public class CreateGameValidator : AbstractValidator<CreateGameCommand>
{
    private static readonly ClassificacaoEtaria[] ValoresValidos =
        Enum.GetValues<ClassificacaoEtaria>();

    public CreateGameValidator(IGameRepository repository)
    {
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(async (nome, ct) => !await repository.ExistsByNameAsync(nome, null, ct))
                .WithMessage("Já existe um jogo cadastrado com esse nome.");

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
