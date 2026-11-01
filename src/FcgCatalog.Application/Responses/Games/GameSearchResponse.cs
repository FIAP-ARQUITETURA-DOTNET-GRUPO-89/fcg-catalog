using FcgCatalog.Domain.Enums;

namespace FcgCatalog.Application.Responses.Games;

public record GameSearchResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    DateTime DataLancamento,
    ClassificacaoEtaria ClassificacaoEtaria,
    double? Score
);
