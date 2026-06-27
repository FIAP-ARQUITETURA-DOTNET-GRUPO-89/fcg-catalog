using FcgCatalog.Domain.Enums;

namespace FcgCatalog.Application.Responses.Games;

public record GameResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    DateTime DataLancamento,
    ClassificacaoEtaria ClassificacaoEtaria
);
