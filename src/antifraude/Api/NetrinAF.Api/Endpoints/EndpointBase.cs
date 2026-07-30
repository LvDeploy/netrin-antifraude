using NetrinAF.Api.ApiResults;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Domain.Enums;

namespace NetrinAF.Api.Endpoints
{
    public abstract class EndpointBase
    {
        protected IResult CreateResponse<T>(string correlationId, BaseResponse<T> result)
        {
            if (result.IsSuccess)
            {
                ResultSuccess<T> resultSuccess = new(correlationId, result.Data!);
                return Results.Ok(resultSuccess);
            }

            ResultFailure resultFailure = new(correlationId, [.. result.Errors!.Select(x => x.Detail)!]);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => Results.BadRequest(resultFailure),
                ErrorType.NotFound => Results.NotFound(resultFailure),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
