using MediatR;
using OperationResult;
using ContractStatus = FgcGames.EventContracts.Enums.PaymentStatus;

namespace FcgCatalog.Application.Commands.Library;

public record ProcessPaymentResultCommand(Guid OrderId, ContractStatus Status) : IRequest<Result<bool>>;
