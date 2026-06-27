using MediatR;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed class UpdateGameHandler(IGameRepository repository)
: IRequestHandler<UpdateGameCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Jogo {request.Id} não encontrado.");

        game.Atualizar(request.Nome, request.Descricao, request.DataLancamento, request.ClassificacaoEtaria);
        game.AlterarPreco(request.Preco);

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
