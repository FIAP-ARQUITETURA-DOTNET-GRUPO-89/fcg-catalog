using FluentValidation;
using FcgCatalog.Application.Commands.Reviews;

namespace FcgCatalog.Application.Validators.Reviews;

public class CreateGameReviewValidator : AbstractValidator<CreateGameReviewCommand>
{
    public CreateGameReviewValidator()
    {
        RuleFor(x => x.JogoId)
            .NotEmpty()
            .WithMessage("O identificador do jogo é obrigatório.");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("O identificador do usuário é obrigatório.");

        RuleFor(x => x.Nota)
            .InclusiveBetween(1, 5)
            .WithMessage("A nota da avaliação deve estar entre 1 e 5.");

        RuleFor(x => x.Comentario)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("O comentário da avaliação é obrigatório e deve possuir no máximo 1000 caracteres.");
    }
}
