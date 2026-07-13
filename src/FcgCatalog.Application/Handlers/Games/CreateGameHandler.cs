using MediatR;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Mappers.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using Microsoft.Extensions.Logging;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed partial class CreateGameHandler(
    IGameRepository repository,
    ILogger<CreateGameHandler> logger)
: IRequestHandler<CreateGameCommand, Result<GameResponse>>
{
    public async Task<Result<GameResponse>> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        LogStarted(logger, request.Nome);

        var game = new Game(request.Nome, request.Descricao, request.Preco, request.DataLancamento, request.ClassificacaoEtaria);

        repository.Add(game);
        await repository.SaveChangesAsync(cancellationToken);

        LogCreated(logger, game.Id, game.Nome);

        return Result.Success(game.ToResponse());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Iniciando a criação do jogo '{Nome}'.")]
    private static partial void LogStarted(ILogger logger, string nome);

    [LoggerMessage(Level = LogLevel.Information, Message = "Jogo '{GameId}' ('{Nome}') criado e persistido com sucesso.")]
    private static partial void LogCreated(ILogger logger, Guid gameId, string nome);
}
