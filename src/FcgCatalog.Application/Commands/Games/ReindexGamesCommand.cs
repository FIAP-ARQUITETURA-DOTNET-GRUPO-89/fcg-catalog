using MediatR;
using OperationResult;

namespace FcgCatalog.Application.Commands.Games;

/// <summary>
/// Reconstrói o índice de busca a partir dos jogos ativos do banco relacional.
/// Retorna a quantidade de jogos indexados.
/// </summary>
/// <param name="OnlyIfOutOfSync">
/// Quando true, reconstrói apenas se o índice não existir ou divergir da quantidade de jogos ativos.
/// </param>
public record ReindexGamesCommand(bool OnlyIfOutOfSync = false) : IRequest<Result<int>>;
