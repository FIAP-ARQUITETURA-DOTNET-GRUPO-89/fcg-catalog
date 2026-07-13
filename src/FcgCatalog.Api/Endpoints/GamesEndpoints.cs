using MediatR;
using FcgCatalog.Api.Extensions;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.SharedKernel.Requests;
using FcgCatalog.SharedKernel.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FcgCatalog.Api.Endpoints;

public static class GamesEndpoints
{
    public static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/games")
            .WithTags("Jogos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateGameAsync)
            .RequireAuthorization("AdminPolicy")
            .WithSummary("Cria um novo jogo")
            .WithDescription("Apenas administradores podem cadastrar jogos no catálogo.")
            .Produces<GameResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetGamesAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithName("GetGames")
            .WithSummary("Lista jogos ativos paginados")
            .Produces<PagedResponse<GameResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetGameByIdAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithName("GetGameById")
            .WithSummary("Obtém um jogo por identificador")
            .Produces<GameResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateGameAsync)
            .RequireAuthorization("AdminPolicy")
            .WithSummary("Atualiza um jogo")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/price", UpdatePriceAsync)
            .RequireAuthorization("AdminPolicy")
            .WithSummary("Altera o preço de um jogo")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteGameAsync)
            .RequireAuthorization("AdminPolicy")
            .WithSummary("Inativa um jogo")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateGameAsync([FromBody] CreateGameCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult(value => $"/api/games/{value!.Id}");
    }

    private static async Task<IResult> GetGamesAsync([AsParameters] PaginationRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetGamesQuery(request.Page, request.PageSize), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetGameByIdAsync([FromRoute] Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetGameByIdQuery(id), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> UpdateGameAsync([FromRoute] Guid id, [FromBody] UpdateGameCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdatePriceAsync([FromRoute] Guid id, [FromBody] UpdatePriceCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteGameAsync([FromRoute] Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteGameCommand(id), cancellationToken);
        return Results.NoContent();
    }
}
