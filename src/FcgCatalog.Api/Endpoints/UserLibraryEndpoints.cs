using System.Security.Claims;
using MediatR;
using FcgCatalog.Api.Extensions;
using FcgCatalog.Application.Commands.Library;
using FcgCatalog.Application.Queries.Library;
using FcgCatalog.Application.Responses.Library;
using Microsoft.AspNetCore.Mvc;

namespace FcgCatalog.Api.Endpoints;

public static class UserLibraryEndpoints
{
    public static void MapUserLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        var libraryGroup = app.MapGroup("api/library")
            .WithTags("Biblioteca")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        var ordersGroup = app.MapGroup("api/orders")
            .WithTags("Pedidos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        ordersGroup.MapPost("/", PurchaseAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Cria um pedido de compra")
            .WithDescription("Cria um pedido de compra e publica um OrderPlacedEvent que será processado pelo PaymentsAPI.")
            .Produces<PurchaseGameResponse>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        libraryGroup.MapGet("/", GetUserLibraryAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Lista os jogos da biblioteca do usuário autenticado")
            .Produces<IReadOnlyList<UserGameResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> PurchaseAsync([FromBody] PurchaseGameCommand command, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken)
    {
        command.UserId = GetUserId(user);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToAcceptedResult(_ => "/api/orders");
    }

    private static async Task<IResult> GetUserLibraryAsync(ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var result = await mediator.Send(new GetUserLibraryQuery(userId), cancellationToken);
        return result.ToOkResult();
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub")
                  ?? user.FindFirstValue(ClaimTypes.Name);

        return Guid.TryParse(raw, out var id)
            ? id
            : throw new UnauthorizedAccessException("Usuário autenticado não possui identificador válido.");
    }
}
