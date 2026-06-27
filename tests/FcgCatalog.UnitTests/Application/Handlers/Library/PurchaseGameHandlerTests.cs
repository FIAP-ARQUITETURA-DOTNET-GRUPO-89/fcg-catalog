using FgcGames.EventContracts.Events;
using MassTransit;
using FcgCatalog.Application.Commands.Library;
using FcgCatalog.Application.Handlers.Library;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Library;

public class PurchaseGameHandlerTests
{
    [Fact]
    public async Task Handle_QuandoJogoNaoExiste_LancaNotFound()
    {
        var gameRepo = Substitute.For<IGameRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();
        var publisher = Substitute.For<IPublishEndpoint>();

        gameRepo.GetByIdAsNoTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Game?)null);

        var handler = new PurchaseGameHandler(gameRepo, orderRepo, userGameRepo, publisher, NullLogger<PurchaseGameHandler>.Instance);

        var command = new PurchaseGameCommand(Guid.NewGuid()) { UserId = Guid.NewGuid() };

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_QuandoUsuarioJaPossui_LancaAlreadyExists()
    {
        var gameRepo = Substitute.For<IGameRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();
        var publisher = Substitute.For<IPublishEndpoint>();

        var game = new Game("Halo", "desc", 100m, DateTime.Today, ClassificacaoEtaria.Livre);
        gameRepo.GetByIdAsNoTrackingAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(game);
        userGameRepo.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = new PurchaseGameHandler(gameRepo, orderRepo, userGameRepo, publisher, NullLogger<PurchaseGameHandler>.Instance);

        var command = new PurchaseGameCommand(game.Id) { UserId = Guid.NewGuid() };

        await Should.ThrowAsync<AlreadyExistsException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveCriarOrderEPublicarOrderPlacedEvent()
    {
        var gameRepo = Substitute.For<IGameRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();
        var userGameRepo = Substitute.For<IUserGameRepository>();
        var publisher = Substitute.For<IPublishEndpoint>();

        var game = new Game("Halo", "desc", 100m, DateTime.Today, ClassificacaoEtaria.Livre);
        gameRepo.GetByIdAsNoTrackingAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        var handler = new PurchaseGameHandler(gameRepo, orderRepo, userGameRepo, publisher, NullLogger<PurchaseGameHandler>.Instance);

        var userId = Guid.NewGuid();
        var command = new PurchaseGameCommand(game.Id) { UserId = userId };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.GameId.ShouldBe(game.Id);
        result.Value.Price.ShouldBe(100m);
        orderRepo.Received(1).Add(Arg.Is<Order>(o => o.UserId == userId && o.GameId == game.Id));
        await publisher.Received(1).Publish(
            Arg.Is<OrderPlacedEvent>(e => e.UserId == userId && e.GameId == game.Id && e.Price == 100m),
            Arg.Any<CancellationToken>());
    }
}
