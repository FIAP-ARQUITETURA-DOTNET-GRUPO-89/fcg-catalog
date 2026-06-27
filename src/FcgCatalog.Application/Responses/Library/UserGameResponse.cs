namespace FcgCatalog.Application.Responses.Library;

public record UserGameResponse(Guid GameId, string Nome, decimal Price, DateTime AcquiredAt);
