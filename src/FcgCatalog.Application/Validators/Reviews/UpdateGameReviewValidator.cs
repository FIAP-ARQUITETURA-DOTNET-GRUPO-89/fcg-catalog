using FluentValidation;
using FcgCatalog.Application.Commands.Reviews;

namespace FcgCatalog.Application.Validators.Reviews;

public class UpdateGameReviewValidator : AbstractValidator<UpdateGameReviewCommand>
{
    public UpdateGameReviewValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O identificador da avaliação é obrigatório.");

        RuleFor(x => x.Nota)
            .InclusiveBetween(1, 5)
            .WithMessage("A nota da avaliação deve estar entre 1 e 5.");

        RuleFor(x => x.Comentario)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("O comentário da avaliação é obrigatório e deve possuir no máximo 1000 caracteres.");
    }
}
