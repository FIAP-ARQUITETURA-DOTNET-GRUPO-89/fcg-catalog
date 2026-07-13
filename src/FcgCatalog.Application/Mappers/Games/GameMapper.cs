using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Application.Mappers.Games;

public static class GameMapper
{
    public static GameResponse ToResponse(this Game game) =>
        new(game.Id, game.Nome, game.Descricao, game.Preco, game.DataLancamento, game.ClassificacaoEtaria);
}
