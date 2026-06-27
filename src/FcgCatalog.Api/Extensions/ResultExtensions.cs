using OperationResult;

namespace FcgCatalog.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToOkResult<T>(this Result<T> result)
        => result.IsSuccess
            ? Results.Ok(result.Value)
            : throw result.Exception!;

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> locationFactory)
        => result.IsSuccess
            ? Results.Created(locationFactory(result.Value!), result.Value)
            : throw result.Exception!;

    public static IResult ToAcceptedResult<T>(this Result<T> result, Func<T, string> locationFactory)
        => result.IsSuccess
            ? Results.Accepted(locationFactory(result.Value!), result.Value)
            : throw result.Exception!;
}
