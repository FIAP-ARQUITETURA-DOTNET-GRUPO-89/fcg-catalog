using FgcGames.EventContracts.Events;
using MassTransit;
using MediatR;
using FcgCatalog.Application.Commands.Library;

namespace FcgCatalog.Worker.Consumers;

public sealed class PaymentProcessedConsumer(
    IMediator mediator,
    ILogger<PaymentProcessedConsumer> logger)
: IConsumer<PaymentProcessedEvent>
{
    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        logger.LogInformation(
            "PaymentProcessed recebido. OrderId: {OrderId}, Status: {Status}.",
            context.Message.OrderId,
            context.Message.Status);

        var command = new ProcessPaymentResultCommand(context.Message.OrderId, context.Message.Status);

        await mediator.Send(command, context.CancellationToken);
    }
}
