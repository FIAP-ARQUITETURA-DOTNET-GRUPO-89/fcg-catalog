namespace FcgCatalog.Application.Responses.Library;

public record PurchaseGameResponse(Guid OrderId, Guid GameId, decimal Price);
