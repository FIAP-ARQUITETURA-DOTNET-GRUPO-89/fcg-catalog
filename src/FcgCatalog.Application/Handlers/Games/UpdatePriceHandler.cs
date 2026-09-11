using MediatR;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed class UpdatePriceHandler(
    IGameRepository repository,
    IGameCacheService cache)
    : IRequestHandler<UpdatePriceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdatePriceCommand request,
        CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Jogo {request.Id} não encontrado.");

        game.AlterarPreco(request.NovoPreco);

        await repository.SaveChangesAsync(
            cancellationToken);

        // Remove o jogo e invalida o cache da listagem.
        await cache.InvalidateGameAsync(
            request.Id);

        return Result.Success(true);
    }
}
