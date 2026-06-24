using FluentValidation;
using FcgCatalog.Application.Queries.Orders;
using FcgCatalog.SharedKernel.Validators;

namespace FcgCatalog.Application.Validators.Orders;

public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>, IValidatableRequest
{
    public GetOrderByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
