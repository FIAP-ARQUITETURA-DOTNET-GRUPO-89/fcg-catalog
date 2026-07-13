using FcgCatalog.Application.Commands.Library;
using FcgCatalog.Application.Handlers.Library;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Domain.Repositories.Orders;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using ContractStatus = FgcGames.EventContracts.Enums.PaymentStatus;

namespace FcgCatalog.UnitTests.Application.Handlers.Library;

public class ProcessPaymentResultHandlerTests
{
    [Fact]
    public async Task Handle_QuandoApproved_AdicionaJogoAoUsuarioEAprovaOrder()
    {
        // Arrange
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();

        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var order = new Order(userId, gameId, 100m);

        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        userGameRepo.ExistsAsync(userId, gameId, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new ProcessPaymentResultHandler(
            orderRepo,
            userGameRepo,
            NullLogger<ProcessPaymentResultHandler>.Instance);

        var command = new ProcessPaymentResultCommand(
            order.Id,
            ContractStatus.Approved);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Approved);

        userGameRepo.Received(1).Add(
            Arg.Is<UserGame>(u =>
                u.UserId == userId &&
                u.GameId == gameId));
    }

    [Fact]
    public async Task Handle_QuandoRejected_RejeitaOrderENaoAdicionaBiblioteca()
    {
        // Arrange
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();

        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m);

        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new ProcessPaymentResultHandler(
            orderRepo,
            userGameRepo,
            NullLogger<ProcessPaymentResultHandler>.Instance);

        var command = new ProcessPaymentResultCommand(
            order.Id,
            ContractStatus.Rejected);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.ShouldBe(OrderStatus.Rejected);
        userGameRepo.DidNotReceive().Add(Arg.Any<UserGame>());
    }

    [Fact]
    public async Task Handle_QuandoOrderJaProcessado_NaoFazNada()
    {
        // Arrange
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();

        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m);

        order.Approve();

        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new ProcessPaymentResultHandler(
            orderRepo,
            userGameRepo,
            NullLogger<ProcessPaymentResultHandler>.Instance);

        var command = new ProcessPaymentResultCommand(
            order.Id,
            ContractStatus.Approved);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        userGameRepo.DidNotReceive().Add(Arg.Any<UserGame>());
    }

    [Fact]
    public async Task Handle_QuandoApproved_E_BibliotecaJaTem_NaoDuplica()
    {
        // Arrange
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();

        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m);

        orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        userGameRepo.ExistsAsync(
                order.UserId,
                order.GameId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new ProcessPaymentResultHandler(
            orderRepo,
            userGameRepo,
            NullLogger<ProcessPaymentResultHandler>.Instance);

        var command = new ProcessPaymentResultCommand(
            order.Id,
            ContractStatus.Approved);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.ShouldBe(OrderStatus.Approved);
        userGameRepo.DidNotReceive().Add(Arg.Any<UserGame>());
    }
}
