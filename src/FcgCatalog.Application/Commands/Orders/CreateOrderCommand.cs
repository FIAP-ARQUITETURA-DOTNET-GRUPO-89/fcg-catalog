using System.Text.Json.Serialization;
using MediatR;
using FcgCatalog.Application.Responses.Orders;
using FcgCatalog.Domain.ValueObjects;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Orders;

public record CreateOrderCommand(
    string Customer,
    decimal TotalAmount,
    string Street,
    string City,
    string State,
    string Cep)
: IRequest<Result<CreateOrderResponse>>, IValidatableRequest
{
    [JsonIgnore]
    public Address DeliveryAddress => new(Street, City, State, Cep);
}
