using MediatR;
using FcgCatalog.Api.Extensions;
using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Application.Queries.Reviews;
using FcgCatalog.Application.Responses.Reviews;
using Microsoft.AspNetCore.Mvc;

namespace FcgCatalog.Api.Endpoints;

public static class GameReviewEndpoints
{
    public static void MapGameReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/games")
            .WithTags("Avaliações")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{jogoId:guid}/reviews", CreateGameReviewAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Cria uma nova avaliação para o jogo")
            .Produces<GameReviewResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{jogoId:guid}/reviews", GetReviewsByGameIdAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithName("GetReviewsByGameId")
            .WithSummary("Lista as avaliações de um jogo específico")
            .Produces<IReadOnlyList<GameReviewResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/reviews/{id:guid}", UpdateGameReviewAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Atualiza uma avaliação existente")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/reviews/{id:guid}", DeleteGameReviewAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Remove uma avaliação")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateGameReviewAsync([FromRoute] Guid jogoId, [FromBody] CreateGameReviewCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var commandWithJogoId = command with { JogoId = jogoId };
        var result = await mediator.Send(commandWithJogoId, cancellationToken);
        return result.ToCreatedResult(value => $"/api/games/{jogoId}/reviews/{value!.Id}");
    }

    private static async Task<IResult> GetReviewsByGameIdAsync([FromRoute] Guid jogoId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetReviewsByGameIdQuery(jogoId), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> UpdateGameReviewAsync([FromRoute] Guid id, [FromBody] UpdateGameReviewCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteGameReviewAsync([FromRoute] Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteGameReviewCommand(id), cancellationToken);
        return Results.NoContent();
    }
}
