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
            .NotEmpty().WithMessage("O nome do jogo é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.")
            .MustAsync(async (nome, ct) => !await repository.ExistsByNameAsync(nome, null, ct))
                .WithMessage("Já existe um jogo cadastrado com esse nome.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descrição do jogo é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.")
            .LessThanOrEqualTo(9999.99m).WithMessage("O preço não pode ser superior a R$ 9.999,99.")
            .PrecisionScale(7, 2, false).WithMessage("O preço deve ter no máximo 2 casas decimais.");

        RuleFor(x => x.DataLancamento)
            .NotEmpty().WithMessage("A data de lançamento é obrigatória.");

        RuleFor(x => x.ClassificacaoEtaria)
            .Must(valor => ValoresValidos.Contains(valor))
                .WithMessage($"Classificação etária inválida. Valores aceitos: {string.Join(", ", ValoresValidos.Select(v => $"{(int)v} ({v})"))}.");
    }
}
