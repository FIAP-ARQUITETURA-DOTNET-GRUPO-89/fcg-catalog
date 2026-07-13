using MediatR;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed partial class UpdateGameHandler(
    IGameRepository repository,
    ILogger<UpdateGameHandler> logger)
    : IRequestHandler<UpdateGameCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (game is null)
        {
            LogGameNotFound(logger, request.Id);
            throw new NotFoundException($"Jogo {request.Id} não encontrado.");
        }

        if (await repository.ExistsByNameAsync(request.Nome, request.Id, cancellationToken))
        {
            LogGameNameAlreadyExists(logger, request.Nome);
            throw new AlreadyExistsException("Já existe um jogo cadastrado com esse nome.");
        }

        game.Atualizar(
            request.Nome,
            request.Descricao,
            request.DataLancamento,
            request.ClassificacaoEtaria);

        game.AlterarPreco(request.Preco);

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Atualização não realizada. Jogo {GameId} não encontrado.")]
    private static partial void LogGameNotFound(
        ILogger logger,
        Guid gameId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Atualização não realizada. Já existe um jogo cadastrado com o nome {GameName}.")]
    private static partial void LogGameNameAlreadyExists(
        ILogger logger,
        string gameName);
}
